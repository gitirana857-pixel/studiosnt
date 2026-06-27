using Android.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WoWonder.Helpers.Utils;

namespace WoWonder.Activities.Live.Rtc
{
    /// <summary>
    /// LiveKit Signaling Protocol Client
    /// 
    /// Gerencia a comunicação WebSocket com o servidor LiveKit.
    /// O protocolo nativo usa mensagens Protobuf binárias (SignalRequest / SignalResponse).
    /// Esta implementação oferece:
    ///   - Conexão/reconexão WebSocket com heartbeat
    ///   - Envio/recebimento de mensagens de sinalização
    ///   - Hooks para integração futura com WebRTC (Offer/Answer/ICE)
    ///   - Suporte a fallback JSON para ambientes sem Protobuf
    /// 
    /// Para integração total com o LiveKit, será necessário:
    ///   1. Adicionar Google.Protobuf NuGet
    ///   2. Compilar livekit.proto → C#
    ///   3. Substituir SendMessage/HandleMessage por serialização real
    /// </summary>
    public class LiveKitSignalingClient : IDisposable
    {
        // ====================================================================
        // Eventos públicos
        // ====================================================================
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;
        public event Action<string> OnRoomJoined;
        public event Action<string> OnParticipantJoined;
        public event Action<string> OnParticipantLeft;
        public event Action<string, string> OnTrackPublished;    // (sid, kind)
        public event Action<string, string> OnTrackSubscribed;   // (sid, kind)
        public event Action<string> OnTrackUnsubscribed;
        public event Action<string> OnTrackMuted;
        public event Action<string> OnTrackUnmuted;
        /// <summary> Disparado quando o servidor envia uma Offer SDP </summary>
        public event Action<string> OnRemoteOffer;
        /// <summary> Disparado quando recebemos ICE candidates remotos </summary>
        public event Action<string, int, string> OnRemoteIceCandidate;
        /// <summary> Disparado quando o servidor confirma nosso Join </summary>
        public event Action<string> OnJoinConfirmed;

        // ====================================================================
        // Propriedades
        // ====================================================================
        public string ServerUrl { get; private set; }
        public string Token { get; private set; }
        public string RoomName { get; private set; }
        public string Identity { get; private set; }
        public string ParticipantSid { get; private set; }
        public bool IsConnected => _webSocket?.State == WebSocketState.Open;
        public LiveKitConnectionState State { get; private set; } = LiveKitConnectionState.Disconnected;

        // ====================================================================
        // Estado interno
        // ====================================================================
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private readonly ConcurrentQueue<byte[]> _sendQueue = new ConcurrentQueue<byte[]>();
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _heartbeatTask;
        private const int HeartbeatIntervalMs = 15000;
        private const int MaxReconnectAttempts = 5;
        private int _reconnectAttempt;
        private bool _disposed;

        // ====================================================================
        // Constantes do protocolo LiveKit
        // ====================================================================

        // MessageType tags (1 byte prefix)
        private const byte MessageTypeSignalRequest = 0x00;
        private const byte MessageTypeSignalResponse = 0x01;

        // SignalRequest message types (LiveKit protocol)
        private const string SignalMethodJoin = "join";
        private const string SignalMethodOffer = "offer";
        private const string SignalMethodAnswer = "answer";
        private const string SignalMethodTrickle = "trickle";
        private const string SignalMethodAddTrack = "add_track";
        private const string SignalMethodMuteTrack = "mute_track";
        private const string SignalMethodLeave = "leave";
        private const string SignalMethodUpdateParticipant = "update_participant";
        private const string SignalMethodUpdateTrackSettings = "update_track_settings";
        private const string SignalMethodSyncState = "sync_state";

        // ====================================================================
        // Construtor & Conexão
        // ====================================================================

        public LiveKitSignalingClient()
        {
        }

        /// <summary>
        /// Conecta ao servidor LiveKit via WebSocket.
        /// </summary>
        public async Task<bool> ConnectAsync(string serverUrl, string token, string roomName, string identity)
        {
            try
            {
                State = LiveKitConnectionState.Connecting;
                ServerUrl = serverUrl;
                Token = token;
                RoomName = roomName;
                Identity = identity;
                _cts = new CancellationTokenSource();

                // Converte http→ws e monta URL de sinalização
                var wsUrl = serverUrl
                    .Replace("https://", "wss://")
                    .Replace("http://", "ws://") + "/rtc?protocol=1&version=1";

                _webSocket = new ClientWebSocket();
                _webSocket.Options.SetRequestHeader("Authorization", "Bearer " + token);

                Log.Debug("LiveKitSignaling", $"Connecting to {wsUrl}");
                await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);

                State = LiveKitConnectionState.Connected;
                _reconnectAttempt = 0;
                OnConnected?.Invoke();

                // Inicia loops
                _receiveLoopTask = ReceiveLoopAsync(_cts.Token);
                _sendLoopTask = SendLoopAsync(_cts.Token);
                _heartbeatTask = HeartbeatLoopAsync(_cts.Token);

                // Envia JoinRequest
                await SendJoinRequestAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Error("LiveKitSignaling", $"Connect failed: {e.Message}");
                State = LiveKitConnectionState.Disconnected;
                OnError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Desconecta do servidor LiveKit.
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                State = LiveKitConnectionState.Disconnecting;

                // Envia LeaveRequest
                await SendJsonMessageAsync(SignalMethodLeave, new { });

                _cts?.Cancel();

                if (_webSocket?.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client disconnect",
                        CancellationToken.None);
                }

                _webSocket?.Dispose();
                _webSocket = null;

                State = LiveKitConnectionState.Disconnected;
                OnDisconnected?.Invoke("Client requested");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                State = LiveKitConnectionState.Disconnected;
            }
        }

        // ====================================================================
        // Envio de mensagens
        // ====================================================================

        /// <summary>
        /// Envia JoinRequest com o token JWT.
        /// </summary>
        private async Task SendJoinRequestAsync()
        {
            // LiveKit espera SignalRequest.join = JoinRequest { token, room, identity, ... }
            // Como não temos protobuf, enviamos como JSON.
            // Em produção, usar protobuf binário.
            var joinPayload = new Dictionary<string, object>
            {
                ["token"] = Token,
                ["room"] = RoomName,
                ["identity"] = Identity,
                ["reconnect"] = _reconnectAttempt > 0,
                ["max_retries"] = MaxReconnectAttempts
            };

            await SendJsonMessageAsync(SignalMethodJoin, joinPayload);
        }

        /// <summary>
        /// Envia SDP Offer (broadcaster publica tracks).
        /// </summary>
        public async Task SendOfferAsync(string sdp)
        {
            await SendJsonMessageAsync(SignalMethodOffer, new
            {
                sdp = sdp,
                type = "offer"
            });
        }

        /// <summary>
        /// Envia SDP Answer (viewer responde à offer).
        /// </summary>
        public async Task SendAnswerAsync(string sdp)
        {
            await SendJsonMessageAsync(SignalMethodAnswer, new
            {
                sdp = sdp,
                type = "answer"
            });
        }

        /// <summary>
        /// Envia ICE candidate (Trickle).
        /// </summary>
        public async Task SendIceCandidateAsync(string candidate, int mlineIndex, string sdpMid)
        {
            await SendJsonMessageAsync(SignalMethodTrickle, new
            {
                candidate = new
                {
                    candidate = candidate,
                    sdpMLineIndex = mlineIndex,
                    sdpMid = sdpMid
                },
                target = 0  // 0 = publisher, 1 = subscriber
            });
        }

        /// <summary>
        /// Notifica servidor sobre mute/unmute de track.
        /// </summary>
        public async Task SendMuteTrackAsync(string trackSid, bool muted)
        {
            await SendJsonMessageAsync(SignalMethodMuteTrack, new
            {
                sid = trackSid,
                muted = muted
            });
        }

        /// <summary>
        /// Envia solicitação para adicionar track (áudio/vídeo).
        /// </summary>
        public async Task SendAddTrackAsync(string cid, string kind, string label, Dictionary<string, string> source = null)
        {
            await SendJsonMessageAsync(SignalMethodAddTrack, new
            {
                cid = cid,
                kind = kind,       // "audio" ou "video"
                label = label,
                source = source ?? new Dictionary<string, string>(),
                type = 0          // 0 = kTrackTypeAudio, 1 = kTrackTypeVideo
            });
        }

        /// <summary>
        /// Envia uma mensagem JSON para o WebSocket.
        /// </summary>
        private async Task SendJsonMessageAsync(string method, object payload)
        {
            try
            {
                var message = new Dictionary<string, object>
                {
                    ["method"] = method,
                    ["payload"] = payload
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                var bytes = Encoding.UTF8.GetBytes(json);

                // Enfileira para o send loop
                _sendQueue.Enqueue(bytes);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // ====================================================================
        // Loops de fundo
        // ====================================================================

        /// <summary>
        /// Loop que envia mensagens enfileiradas.
        /// </summary>
        private async Task SendLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
                {
                    if (_sendQueue.TryDequeue(out var data))
                    {
                        await _webSocket.SendAsync(
                            new ArraySegment<byte>(data),
                            WebSocketMessageType.Text,
                            true,
                            ct);
                    }
                    else
                    {
                        await Task.Delay(50, ct);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Log.Error("LiveKitSignaling", $"SendLoop error: {e.Message}");
            }
        }

        /// <summary>
        /// Loop que recebe mensagens do WebSocket.
        /// </summary>
        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[8192];
            var messageBuffer = new List<byte>();

            try
            {
                while (!ct.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log.Debug("LiveKitSignaling", "Server closed connection");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Protocolo nativo LiveKit (Protobuf)
                        // Em produção, fazer parse com Google.Protobuf
                        messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                        if (result.EndOfMessage)
                        {
                            HandleBinaryMessage(messageBuffer.ToArray());
                            messageBuffer.Clear();
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Fallback JSON
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        HandleTextMessage(text);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException we)
            {
                Log.Error("LiveKitSignaling", $"WebSocket error: {we.Message}");
                OnError?.Invoke(we.Message);
            }
            catch (Exception e)
            {
                Log.Error("LiveKitSignaling", $"Receive error: {e.Message}");
            }
            finally
            {
                // Tenta reconectar se não foi disconnect intencional
                if (State == LiveKitConnectionState.Connected ||
                    State == LiveKitConnectionState.Reconnecting)
                {
                    _ = TryReconnectAsync();
                }
            }
        }

        /// <summary>
        /// Heartbeat para manter conexão ativa.
        /// </summary>
        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(HeartbeatIntervalMs, ct);
                    if (_webSocket?.State == WebSocketState.Open)
                    {
                        // Ping implícito via send queue
                        await SendJsonMessageAsync("ping", new { });
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        // ====================================================================
        // Processamento de mensagens
        // ====================================================================

        /// <summary>
        /// Processa mensagem binária (Protocolo nativo LiveKit/Protobuf).
        /// Em produção, usar Google.Protobuf para deserializar SignalResponse.
        /// </summary>
        private void HandleBinaryMessage(byte[] data)
        {
            try
            {
                if (data.Length < 1) return;

                var messageType = data[0];

                // O payload restante é o protobuf serializado
                // SignalResponse pode conter:
                //   - join_response (JoinResponse)
                //   - answer (SessionDescription)
                //   - offer (SessionDescription)
                //   - trickle (TrickleRequest)
                //   - update (ParticipantUpdate)
                //   - track_published (TrackPublishedResponse)
                //   - mute_track (MuteTrackCallback)
                //   - leave (LeaveRequest)
                //   - sync_state (SyncState)
                //   - ...

                switch (messageType)
                {
                    case MessageTypeSignalResponse:
                        Log.Debug("LiveKitSignaling", $"Received SignalResponse ({data.Length} bytes)");
                        // TODO: Deserializar com Google.Protobuf quando disponível
                        // var response = SignalResponse.Parser.ParseFrom(data, 1, data.Length - 1);
                        // DispatchSignalResponse(response);
                        break;

                    default:
                        Log.Warn("LiveKitSignaling", $"Unknown message type: {messageType}");
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Processa mensagem texto (JSON fallback).
        /// </summary>
        private void HandleTextMessage(string json)
        {
            try
            {
                Log.Debug("LiveKitSignaling", $"JSON message: {json}");

                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (message == null) return;

                var method = message.GetValueOrDefault("method")?.ToString();
                var payload = message.ContainsKey("payload")
                    ? message["payload"] as Newtonsoft.Json.Linq.JToken
                    : null;

                if (string.IsNullOrEmpty(method)) return;

                switch (method)
                {
                    case "join_response":
                    case "join":
                        HandleJoinResponse(payload);
                        break;

                    case "offer":
                        HandleOfferMessage(payload);
                        break;

                    case "answer":
                        HandleAnswerMessage(payload);
                        break;

                    case "trickle":
                        HandleTrickleMessage(payload);
                        break;

                    case "participant_joined":
                        HandleParticipantJoined(payload);
                        break;

                    case "participant_left":
                        HandleParticipantLeft(payload);
                        break;

                    case "track_published":
                        HandleTrackPublished(payload);
                        break;

                    case "track_unpublished":
                        HandleTrackUnpublished(payload);
                        break;

                    case "track_subscribed":
                        HandleTrackSubscribed(payload);
                        break;

                    case "track_unsubscribed":
                        HandleTrackUnsubscribed(payload);
                        break;

                    case "mute_changed":
                        HandleMuteChanged(payload);
                        break;

                    case "pong":
                        // Heartbeat response - ignore
                        break;

                    case "error":
                        var errorMsg = payload?.ToString();
                        OnError?.Invoke(errorMsg ?? "Unknown error");
                        break;

                    default:
                        Log.Debug("LiveKitSignaling", $"Unhandled method: {method}");
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // ====================================================================
        // Handlers de sinalização
        // ====================================================================

        private void HandleJoinResponse(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;

                var participant = payload["participant"];
                if (participant != null)
                {
                    ParticipantSid = participant["sid"]?.ToString();
                    var identity = participant["identity"]?.ToString();
                    Log.Debug("LiveKitSignaling", $"Joined room as {identity} (sid: {ParticipantSid})");
                    OnJoinConfirmed?.Invoke(ParticipantSid);
                    OnRoomJoined?.Invoke(identity);
                }

                // Outros participantes já na sala
                var otherParticipants = payload["other_participants"] as Newtonsoft.Json.Linq.JArray;
                if (otherParticipants != null)
                {
                    foreach (var p in otherParticipants)
                    {
                        var pid = p["identity"]?.ToString();
                        OnParticipantJoined?.Invoke(pid);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleOfferMessage(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;
                var sdp = payload["sdp"]?.ToString();
                if (!string.IsNullOrEmpty(sdp))
                {
                    Log.Debug("LiveKitSignaling", "Received SDP offer");
                    OnRemoteOffer?.Invoke(sdp);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleAnswerMessage(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;
                var sdp = payload["sdp"]?.ToString();
                if (!string.IsNullOrEmpty(sdp))
                {
                    Log.Debug("LiveKitSignaling", "Received SDP answer");
                    // Answer recebido — conexão WebRTC estabelecida
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleTrickleMessage(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;

                var candidate = payload["candidate"];
                if (candidate == null) return;

                var candidateStr = candidate["candidate"]?.ToString();
                var mlineIndex = candidate["sdpMLineIndex"]?.Value<int>() ?? 0;
                var sdpMid = candidate["sdpMid"]?.ToString();

                if (!string.IsNullOrEmpty(candidateStr))
                {
                    OnRemoteIceCandidate?.Invoke(candidateStr, mlineIndex, sdpMid);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleParticipantJoined(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                var identity = payload?["identity"]?.ToString();
                if (!string.IsNullOrEmpty(identity))
                {
                    Log.Debug("LiveKitSignaling", $"Participant joined: {identity}");
                    OnParticipantJoined?.Invoke(identity);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleParticipantLeft(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                var identity = payload?["identity"]?.ToString();
                if (!string.IsNullOrEmpty(identity))
                {
                    Log.Debug("LiveKitSignaling", $"Participant left: {identity}");
                    OnParticipantLeft?.Invoke(identity);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleTrackPublished(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;
                var sid = payload["track"]?["sid"]?.ToString();
                var kind = payload["track"]?["kind"]?.ToString(); // "audio" ou "video"
                if (!string.IsNullOrEmpty(sid))
                {
                    OnTrackPublished?.Invoke(sid, kind);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleTrackUnpublished(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                var trackSid = payload?["track_sid"]?.ToString();
                if (!string.IsNullOrEmpty(trackSid))
                {
                    OnTrackUnsubscribed?.Invoke(trackSid);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleTrackSubscribed(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;
                var sid = payload["track"]?["sid"]?.ToString();
                var kind = payload["track"]?["kind"]?.ToString();
                if (!string.IsNullOrEmpty(sid))
                {
                    OnTrackSubscribed?.Invoke(sid, kind);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleTrackUnsubscribed(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                var trackSid = payload?["track_sid"]?.ToString();
                if (!string.IsNullOrEmpty(trackSid))
                {
                    OnTrackUnsubscribed?.Invoke(trackSid);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void HandleMuteChanged(Newtonsoft.Json.Linq.JToken payload)
        {
            try
            {
                if (payload == null) return;
                var trackSid = payload["track_sid"]?.ToString();
                var muted = payload["muted"]?.Value<bool>() ?? false;

                if (!string.IsNullOrEmpty(trackSid))
                {
                    if (muted)
                        OnTrackMuted?.Invoke(trackSid);
                    else
                        OnTrackUnmuted?.Invoke(trackSid);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // ====================================================================
        // Reconexão
        // ====================================================================

        /// <summary>
        /// Tenta reconectar com backoff exponencial.
        /// </summary>
        private async Task TryReconnectAsync()
        {
            while (_reconnectAttempt < MaxReconnectAttempts && !_disposed)
            {
                _reconnectAttempt++;
                var delay = Math.Min(1000 * (int)Math.Pow(2, _reconnectAttempt), 30000);
                Log.Debug("LiveKitSignaling", $"Reconnect attempt {_reconnectAttempt}/{MaxReconnectAttempts} in {delay}ms");

                await Task.Delay(delay);

                try
                {
                    State = LiveKitConnectionState.Reconnecting;

                    _webSocket?.Dispose();
                    _webSocket = new ClientWebSocket();
                    _webSocket.Options.SetRequestHeader("Authorization", "Bearer " + Token);

                    var wsUrl = ServerUrl
                        .Replace("https://", "wss://")
                        .Replace("http://", "ws://") + "/rtc?protocol=1&version=1";

                    await _webSocket.ConnectAsync(new Uri(wsUrl), CancellationToken.None);

                    State = LiveKitConnectionState.Connected;
                    _reconnectAttempt = 0;
                    OnConnected?.Invoke();

                    // Re-envia Join
                    await SendJoinRequestAsync();

                    // Reinicia loops
                    _cts = new CancellationTokenSource();
                    _receiveLoopTask = ReceiveLoopAsync(_cts.Token);
                    _sendLoopTask = SendLoopAsync(_cts.Token);
                    _heartbeatTask = HeartbeatLoopAsync(_cts.Token);

                    return;
                }
                catch (Exception e)
                {
                    Log.Error("LiveKitSignaling", $"Reconnect attempt {_reconnectAttempt} failed: {e.Message}");
                }
            }

            // Esgotou tentativas
            State = LiveKitConnectionState.Disconnected;
            OnDisconnected?.Invoke("Max reconnection attempts reached");
        }

        // ====================================================================
        // Dispose
        // ====================================================================

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts?.Cancel();
            _cts?.Dispose();
            _webSocket?.Dispose();

            _sendQueue.Clear();
        }

        /// <summary>
        /// Reinicia o cliente para reuso.
        /// </summary>
        public void Reset()
        {
            _ = DisconnectAsync();
            _reconnectAttempt = 0;
            _sendQueue.Clear();
            ParticipantSid = null;
        }
    }
}
