using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using IO.Agora.Rtc2;
using JetBrains.Annotations;
using System;
using System.Text;

namespace WoWonder.Activities.Live.Ui
{
    public class VideoReportLayout : FrameLayout, View.IOnAttachStateChangeListener
    {
        private readonly StatisticsInfo StatisticsInfo = new StatisticsInfo();
        private TextView ReportTextView;
        private int ReportUid = -1;

        protected VideoReportLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public VideoReportLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        public VideoReportLayout([NotNull] Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public VideoReportLayout([NotNull] Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public VideoReportLayout([NotNull] Context context) : base(context)
        {
        }

        public override void AddView(View child, int index, ViewGroup.LayoutParams @params)
        {
            try
            {
                base.AddView(child, index, @params);
                if (child is SurfaceView || child is TextureView)
                {
                    ReportTextView = new TextView(Context);
                    ReportTextView.AddOnAttachStateChangeListener(this);

                    ReportTextView.SetTextColor(Color.ParseColor("#eeeeee"));
                    LayoutParams reportParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    reportParams.TopMargin = 16;
                    reportParams.LeftMargin = 16;
                    AddView(ReportTextView, reportParams);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Sets report uid
        /// </summary>
        /// <param name="uid">the report uid</param>
        public void SetReportUid(int uid)
        {
            ReportUid = uid;
        }

        /// <summary>
        /// Gets report uid
        /// </summary>
        /// <returns>the report uid</returns>
        public int GetReportUid()
        {
            return ReportUid;
        }

        /// <summary>
        /// Set local video stats
        /// </summary>
        /// <param name="stats">the stats</param>
        public void SetLocalAudioStats(IRtcEngineEventHandler.LocalAudioStats stats)
        {
            StatisticsInfo.SetLocalAudioStats(stats);
            SetReportText(StatisticsInfo.GetLocalVideoStats());
        }

        /// <summary>
        /// Set remote audio stats
        /// </summary>
        /// <param name="stats">the stats</param>
        public void SetLocalVideoStats(IRtcEngineEventHandler.LocalVideoStats stats)
        {
            if (stats.Uid != ReportUid)
            {
                return;
            }
            StatisticsInfo.SetLocalVideoStats(stats);
            SetReportText(StatisticsInfo.GetLocalVideoStats());
        }

        public void SetRemoteVideoStats(IRtcEngineEventHandler.RemoteVideoStats stats)
        {
            if (stats.Uid != ReportUid)
            {
                return;
            }
            StatisticsInfo.SetRemoteVideoStats(stats);
            SetReportText(StatisticsInfo.GetRemoteVideoStats());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportText"></param>
        private void SetReportText(string reportText)
        {
            if (ReportTextView != null)
            {
                ReportTextView.Post(() =>
                {
                    if (ReportTextView != null)
                    {
                        ReportTextView.Text = reportText;
                    }
                });
            }
        }

        public void OnViewAttachedToWindow(View attachedView)
        {

        }

        public void OnViewDetachedFromWindow(View detachedView)
        {
            if (ReportTextView != null)
            {
                ReportTextView.RemoveOnAttachStateChangeListener(this);
                ReportTextView = null;
            }
        }
    }

    public class StatisticsInfo
    {
        private IRtcEngineEventHandler.LocalVideoStats LocalVideoStats = new IRtcEngineEventHandler.LocalVideoStats();
        private IRtcEngineEventHandler.LocalAudioStats LocalAudioStats = new IRtcEngineEventHandler.LocalAudioStats();
        private IRtcEngineEventHandler.RemoteVideoStats RemoteVideoStats = new IRtcEngineEventHandler.RemoteVideoStats();
        private IRtcEngineEventHandler.RemoteAudioStats RemoteAudioStats = new IRtcEngineEventHandler.RemoteAudioStats();
        private IRtcEngineEventHandler.RtcStats RtcStats = new IRtcEngineEventHandler.RtcStats();
        private int Quality;
        private IRtcEngineEventHandler.LastmileProbeResult LastMileProbeResult;

        /// <summary>
        /// Sets local video stats
        /// </summary>
        /// <param name="localVideoStats">the local video stats</param>
        public void SetLocalVideoStats(IRtcEngineEventHandler.LocalVideoStats localVideoStats)
        {
            LocalVideoStats = localVideoStats;
        }

        /// <summary>
        /// Sets local audio stats
        /// </summary>
        /// <param name="localAudioStats">the local audio stats</param>
        public void SetLocalAudioStats(IRtcEngineEventHandler.LocalAudioStats localAudioStats)
        {
            LocalAudioStats = localAudioStats;
        }

        /// <summary>
        /// Sets remote video stats
        /// </summary>
        /// <param name="remoteVideoStats">the remote video stats</param>
        public void SetRemoteVideoStats(IRtcEngineEventHandler.RemoteVideoStats remoteVideoStats)
        {
            RemoteVideoStats = remoteVideoStats;
        }

        /// <summary>
        /// Sets remote audio stats.
        /// </summary>
        /// <param name="remoteAudioStats">the remote audio stats</param>
        public void SetRemoteAudioStats(IRtcEngineEventHandler.RemoteAudioStats remoteAudioStats)
        {
            RemoteAudioStats = remoteAudioStats;
        }

        /// <summary>
        /// Sets rtc stats
        /// </summary>
        /// <param name="rtcStats">the rtc stats</param>
        public void SetRtcStats(IRtcEngineEventHandler.RtcStats rtcStats)
        {
            RtcStats = rtcStats;
        }

        /// <summary>
        /// Gets local video stats
        /// </summary>
        /// <returns>the local video stats</returns>
        public string GetLocalVideoStats()
        {
            StringBuilder builder = new StringBuilder();
            return builder
                .Append("" + LocalVideoStats.EncodedFrameWidth)
                .Append("×")
                .Append(LocalVideoStats.EncodedFrameHeight)
                .Append(",")
                .Append(LocalVideoStats.EncoderOutputFrameRate)
                .Append("fps")
                .Append("\n")
                .Append("LM Delay: ")
                .Append(RtcStats.LastmileDelay)
                .Append("ms")
                .Append("\n")
                .Append("VSend: ")
                .Append(LocalVideoStats.SentBitrate)
                .Append("kbps")
                .Append("\n")
                .Append("ASend: ")
                .Append(LocalAudioStats.SentBitrate)
                .Append("kbps")
                .Append("\n")
                .Append("CPU: ")
                .Append(RtcStats.CpuAppUsage)
                .Append("%/")
                .Append(RtcStats.CpuTotalUsage)
                .Append("%/")
                .Append("\n")
                .Append("VSend Loss: ")
                .Append(RtcStats.TxPacketLossRate)
                .Append("%")
                .ToString();
        }

        /// <summary>
        /// Gets remote video stats
        /// </summary>
        /// <returns>the remote video stats</returns>
        public string GetRemoteVideoStats()
        {
            StringBuilder builder = new StringBuilder();
            return builder
                .Append(RemoteVideoStats.Width)
                .Append("×")
                .Append(RemoteVideoStats.Height)
                .Append(",")
                .Append(RemoteVideoStats.RendererOutputFrameRate)
                .Append("fps")
                .Append("\n")
                .Append("VRecv: ")
                .Append(RemoteVideoStats.ReceivedBitrate)
                .Append("kbps")
                .Append("\n")
                .Append("ARecv: ")
                .Append(RemoteAudioStats.ReceivedBitrate)
                .Append("kbps")
                .Append("\n")
                .Append("VLoss: ")
                .Append(RemoteVideoStats.PacketLossRate)
                .Append("%")
                .Append("\n")
                .Append("ALoss: ")
                .Append(RemoteAudioStats.AudioLossRate)
                .Append("%")
                .Append("\n")
                .Append("AQuality: ")
                .Append(RemoteAudioStats.Quality)
                .ToString();
        }

        /// <summary>
        /// Sets last mile quality
        /// </summary>
        /// <param name="quality">the quality</param>
        public void SetLastMileQuality(int quality)
        {
            Quality = quality;
        }

        /// <summary>
        /// Get last mile quality string
        /// </summary>
        /// <returns>the string</returns>
        public string GetLastMileQuality()
        {
            switch (Quality)
            {
                case 1:
                    return "EXCELLENT";
                case 2:
                    return "GOOD";
                case 3:
                    return "POOR";
                case 4:
                    return "BAD";
                case 5:
                    return "VERY BAD";
                case 6:
                    return "DOWN";
                case 7:
                    return "UNSUPPORTED";
                case 8:
                    return "DETECTING";
                default:
                    return "UNKNOWN";
            }
        }

        /// <summary>
        /// Gets last mile result
        /// </summary>
        /// <returns>the last mile result</returns>
        public string GetLastMileResult()
        {
            if (LastMileProbeResult == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Rtt: ")
                .Append(LastMileProbeResult.Rtt)
                .Append("ms")
                .Append("\n")
                .Append("DownlinkAvailableBandwidth: ")
                .Append(LastMileProbeResult.DownlinkReport.AvailableBandwidth)
                .Append("Kbps")
                .Append("\n")
                .Append("DownlinkJitter: ")
                .Append(LastMileProbeResult.DownlinkReport.Jitter)
                .Append("ms")
                .Append("\n")
                .Append("DownlinkLoss: ")
                .Append(LastMileProbeResult.DownlinkReport.PacketLossRate)
                .Append("%")
                .Append("\n")
                .Append("UplinkAvailableBandwidth: ")
                .Append(LastMileProbeResult.UplinkReport.AvailableBandwidth)
                .Append("Kbps")
                .Append("\n")
                .Append("UplinkJitter: ")
                .Append(LastMileProbeResult.UplinkReport.Jitter)
                .Append("ms")
                .Append("\n")
                .Append("UplinkLoss: ")
                .Append(LastMileProbeResult.UplinkReport.PacketLossRate)
                .Append("%");
            return stringBuilder.ToString();
        }


        /// <summary>
        /// Sets last mile probe result
        /// </summary>
        /// <param name="lastmileProbeResult">the lastmile probe result</param>
        public void SetLastMileProbeResult(IRtcEngineEventHandler.LastmileProbeResult lastmileProbeResult)
        {
            LastMileProbeResult = lastmileProbeResult;
        }
    }
}
