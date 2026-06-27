using Android.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Live.Rtc
{
    public enum LiveKitConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Disconnecting
    }

    public class LiveKitRoomManager : IDisposable
    {
        private static LiveKitRoomManager _instance;
        public static LiveKitRoomManager Instance => _instance ?? (_instance = new LiveKitRoomManager());

        private LiveKitSignalingClient _signalingClient;
        private string _token;
        private string _serverUrl;
        private string _roomName;
        private string _identity;

        public LiveKitConnectionState ConnectionState { get; private set; } = LiveKitConnectionState.Disconnected;
        public string RoomName => _roomName;
        public string Identity => _identity;

        public string CurrentToken { get; set; }
        private bool _audioEnabled = true;
        private bool _videoEnabled = true;

        public event Action<string, LiveKitConnectionState> OnConnectionStateChanged;
        public event Action<string> OnRoomJoined;
        public event Action<string> OnParticipantJoined;
        public event Action<string> OnParticipantLeft;
        public event Action<string, string> OnTrackPublished;   // (sid, kind)
        public event Action<string, string> OnTrackSubscribed;  // (sid, kind)

        // ====================================================================
        // Conexão / Desconexão
        // ====================================================================

        public async Task<bool> JoinRoomAsync(string serverUrl, string token, string roomName, string identity)
        {
            try
            {
                ConnectionState = LiveKitConnectionState.Connecting;
                OnConnectionStateChanged?.Invoke(roomName, ConnectionState);

                _serverUrl = serverUrl;
                _token = token;
                _roomName = roomName;
                _identity = identity;

                _signalingClient = new LiveKitSignalingClient();
                WireSignalingEvents();

                var success = await _signalingClient.ConnectAsync(serverUrl, token, roomName, identity);

                if (success)
                {
                    ConnectionState = LiveKitConnectionState.Connected;
                    OnConnectionStateChanged?.Invoke(roomName, ConnectionState);
                    OnRoomJoined?.Invoke(roomName);
                }
                else
                {
                    ConnectionState = LiveKitConnectionState.Disconnected;
                    OnConnectionStateChanged?.Invoke(roomName, ConnectionState);
                }

                return success;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                ConnectionState = LiveKitConnectionState.Disconnected;
                OnConnectionStateChanged?.Invoke(roomName, ConnectionState);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                ConnectionState = LiveKitConnectionState.Disconnecting;
                OnConnectionStateChanged?.Invoke(_roomName, ConnectionState);

                if (_signalingClient != null)
                {
                    await _signalingClient.DisconnectAsync();
                    _signalingClient.Dispose();
                    _signalingClient = null;
                }

                ConnectionState = LiveKitConnectionState.Disconnected;
                OnConnectionStateChanged?.Invoke(_roomName, ConnectionState);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                ConnectionState = LiveKitConnectionState.Disconnected;
            }
        }

        // ====================================================================
        // Eventos do Signaling Client
        // ====================================================================

        private void WireSignalingEvents()
        {
            if (_signalingClient == null) return;

            _signalingClient.OnConnected += () =>
            {
                Log.Debug("LiveKit", "Signaling connected");
            };

            _signalingClient.OnDisconnected += (reason) =>
            {
                Log.Debug("LiveKit", $"Signaling disconnected: {reason}");
                ConnectionState = LiveKitConnectionState.Disconnected;
                OnConnectionStateChanged?.Invoke(_roomName, ConnectionState);
            };

            _signalingClient.OnRoomJoined += (identity) =>
            {
                Log.Debug("LiveKit", $"Joined room as {identity}");
                OnRoomJoined?.Invoke(_roomName);
            };

            _signalingClient.OnParticipantJoined += (identity) =>
            {
                Log.Debug("LiveKit", $"Participant joined: {identity}");
                OnParticipantJoined?.Invoke(identity);
            };

            _signalingClient.OnParticipantLeft += (identity) =>
            {
                Log.Debug("LiveKit", $"Participant left: {identity}");
                OnParticipantLeft?.Invoke(identity);
            };

            _signalingClient.OnTrackPublished += (sid, kind) =>
            {
                Log.Debug("LiveKit", $"Track published: {sid} ({kind})");
                OnTrackPublished?.Invoke(sid, kind);
            };

            _signalingClient.OnTrackSubscribed += (sid, kind) =>
            {
                Log.Debug("LiveKit", $"Track subscribed: {sid} ({kind})");
                OnTrackSubscribed?.Invoke(sid, kind);
            };

            _signalingClient.OnError += (error) =>
            {
                Log.Error("LiveKit", $"Signaling error: {error}");
            };
        }

        // ====================================================================
        // Controles de mídia
        // ====================================================================

        public void SetAudioEnabled(bool enabled)
        {
            try
            {
                _audioEnabled = enabled;
                // LiveKit: mute/unmute local audio track publication
                _signalingClient?.SendMuteTrackAsync("local_audio", !enabled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetVideoEnabled(bool enabled)
        {
            try
            {
                _videoEnabled = enabled;
                // LiveKit: enable/disable local video track publication
                _signalingClient?.SendMuteTrackAsync("local_video", !enabled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SwitchCamera()
        {
            try
            {
                // LiveKit: switch between front/back camera
                // implementation depends on LiveKit SDK / CameraX binding
                Log.Debug("LiveKit", "Camera switch requested");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ToggleBeautyEffect(bool enabled)
        {
            try
            {
                // LiveKit or Android native beauty filter
                // Requires camera preprocessing integration
                Log.Debug("LiveKit", $"Beauty effect: {enabled}");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // ====================================================================
        // Criação de Live (conexão com PHP backend)
        // ====================================================================

        public async Task<Dictionary<string, object>> CreateLiveStreamAsync(string streamName)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Posts.CreateLiveAsync(streamName);
                if (apiStatus == 200)
                {
                    // O PHP retorna os dados dentro de "data"
                    if (respond.TryGetValue("data", out var dataRaw) && dataRaw is Dictionary<string, object> data)
                    {
                        var result = new Dictionary<string, object>
                        {
                            ["post_id"] = data.GetValueOrDefault("post_id", ""),
                            ["livekit_token"] = data.GetValueOrDefault("livekit_token", ""),
                            ["livekit_url"] = data.GetValueOrDefault("livekit_url", ""),
                            ["livekit_identity"] = data.GetValueOrDefault("livekit_identity", ""),
                            ["livekit_room"] = data.GetValueOrDefault("livekit_room", "")
                        };
                        return result;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        // ====================================================================
        // Sinalização WebRTC (para integração futura com SDK)
        // ====================================================================

        /// <summary>
        /// Envia SDP Offer para o servidor LiveKit.
        /// Usado pelo broadcaster para publicar tracks.
        /// </summary>
        public async Task SendOfferAsync(string sdp)
        {
            if (_signalingClient != null)
                await _signalingClient.SendOfferAsync(sdp);
        }

        /// <summary>
        /// Envia SDP Answer para o servidor.
        /// Usado pelo viewer ao receber uma offer.
        /// </summary>
        public async Task SendAnswerAsync(string sdp)
        {
            if (_signalingClient != null)
                await _signalingClient.SendAnswerAsync(sdp);
        }

        /// <summary>
        /// Envia ICE candidate.
        /// </summary>
        public async Task SendIceCandidateAsync(string candidate, int mlineIndex, string sdpMid)
        {
            if (_signalingClient != null)
                await _signalingClient.SendIceCandidateAsync(candidate, mlineIndex, sdpMid);
        }

        // ====================================================================
        // Dispose
        // ====================================================================

        public void Dispose()
        {
            _signalingClient?.Dispose();
            _signalingClient = null;
        }
    }
}
