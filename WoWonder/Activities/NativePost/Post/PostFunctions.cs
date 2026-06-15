using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Activities.NativePost.Post
{
    public static class PostFunctions
    {
        public static PostModelType GetAdapterType(PostDataObject item)
        {
            try
            {
                if (item == null) return PostModelType.NormalPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "ad") return PostModelType.AdsPost;

                if (item.SharedInfo.SharedInfoClass != null)
                    return PostModelType.SharedPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "profile_cover_picture" || item.PostType == "profile_picture")
                    return PostModelType.ImagePost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "live" && !string.IsNullOrEmpty(item.StreamName))
                {
                    if (ListUtils.SettingsSiteList?.AgoraLiveVideo is "1" && !string.IsNullOrEmpty(ListUtils.SettingsSiteList?.AgoraAppId))
                    {
                        if (item.LiveTime != null && item.LiveTime.Value > 0 && item.IsStillLive != null && item.IsStillLive.Value && string.IsNullOrEmpty(item.AgoraResourceId) && string.IsNullOrEmpty(item.PostFile)) //Live
                            return PostModelType.AgoraLivePost;
                        if (item.LiveTime != null && item.LiveTime.Value > 0 && !string.IsNullOrEmpty(item.AgoraResourceId) && !string.IsNullOrEmpty(item.PostFile)) //Saved
                            return PostModelType.VideoPost;
                        //End
                        return PostModelType.AgoraLivePost;
                    }

                    return PostModelType.LivePost;
                }

                if (!string.IsNullOrEmpty(item.PostFileFull))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(item.PostFileFull);
                    switch (type)
                    {
                        case "Forbidden":
                            return PostModelType.NormalPost;
                        case "Audio":
                            return PostModelType.VoicePost;
                        case "Video":
                            return PostModelType.VideoPost;
                        case "File":
                            return PostModelType.FilePost;
                        case "Image":
                            {
                                var imagesList = item.PhotoMulti ?? item.PhotoAlbum;
                                if (imagesList?.Count > 0)
                                {
                                    switch (imagesList?.Count)
                                    {
                                        case 2:
                                            return PostModelType.MultiImage2;
                                        case 3:
                                            return PostModelType.MultiImage3;
                                        case 4:
                                            return PostModelType.MultiImage4;
                                        case 5:
                                            return PostModelType.MultiImage5;
                                        case 6:
                                            return PostModelType.MultiImage6;
                                        case 7:
                                            return PostModelType.MultiImage7;
                                        case 8:
                                            return PostModelType.MultiImage8;
                                        case 9:
                                            return PostModelType.MultiImage9;
                                        case 10:
                                            return PostModelType.MultiImage10;
                                        case >= 11:
                                            return PostModelType.MultiImages;
                                    }
                                }

                                return PostModelType.ImagePost;
                            }
                    }
                }

                if (item.PhotoMulti?.Count > 0 || item.PhotoAlbum?.Count > 0 || !string.IsNullOrEmpty(item.AlbumName))
                {
                    var imagesList = item.PhotoMulti ?? item.PhotoAlbum;
                    if (imagesList?.Count > 0)
                    {
                        switch (imagesList?.Count)
                        {
                            case 2:
                                return PostModelType.MultiImage2;
                            case 3:
                                return PostModelType.MultiImage3;
                            case 4:
                                return PostModelType.MultiImage4;
                            case 5:
                                return PostModelType.MultiImage5;
                            case 6:
                                return PostModelType.MultiImage6;
                            case 7:
                                return PostModelType.MultiImage7;
                            case 8:
                                return PostModelType.MultiImage8;
                            case 9:
                                return PostModelType.MultiImage9;
                            case 10:
                                return PostModelType.MultiImage10;
                            case >= 11:
                                return PostModelType.MultiImages;
                        }
                    }

                    return PostModelType.ImagePost;
                }

                if (!string.IsNullOrEmpty(item.PostRecord))
                    return PostModelType.VoicePost;

                if (!string.IsNullOrEmpty(item.PostSticker)) return
                    PostModelType.StickerPost;

                if (!string.IsNullOrEmpty(item.PostFacebook)) return
                    PostModelType.FacebookPost;

                if (!string.IsNullOrEmpty(item.PostVimeo)) return
                    PostModelType.VimeoPost;

                if (!string.IsNullOrEmpty(item.PostYoutube)) return
                    PostModelType.YoutubePost;

                if (!string.IsNullOrEmpty(item.PostDeepsound)) return
                    PostModelType.DeepSoundPost;

                if (!string.IsNullOrEmpty(item.PostPlaytube)) return
                    PostModelType.PlayTubePost;

                if (!string.IsNullOrEmpty(item.PostLink) && item.PostLink.Contains("tiktok"))
                    return PostModelType.TikTokPost;
                if (!string.IsNullOrEmpty(item.PostLink) && item.PostLink.Contains("twitter")) return PostModelType.TwitterPost;

                if (!string.IsNullOrEmpty(item.PostLink)) return PostModelType.LinkPost;

                if (item.Product?.ProductClass != null)
                    return PostModelType.ProductPost;

                if (item.Job != null && (item.PostType == "job" || item.Job.Value.JobInfoClass != null))
                    return PostModelType.JobPost;

                if (item.Offer?.OfferClass != null && item.PostType == "offer")
                    return PostModelType.OfferPost;

                if (item.Blog?.BlogClass != null)
                    return PostModelType.BlogPost;

                if (item.Event?.EventClass != null)
                    return PostModelType.EventPost;

                if (item.ColorId != "0")
                    return PostModelType.ColorPost;

                if (item.PollId != "0")
                    return PostModelType.PollPost;

                if (item.FundData?.FundDataClass != null)
                    return PostModelType.FundingPost;

                if (item.Fund?.PurpleFund != null)
                    return PostModelType.PurpleFundPost;

                if (!string.IsNullOrEmpty(item.PostMap))
                    return PostModelType.MapPost;

                return PostModelType.NormalPost;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return PostModelType.NormalPost;
            }
        }
        public static string GetFeelingTypeIcon(string feeling)
        {
            try
            {
                if (string.IsNullOrEmpty(feeling))
                    return string.Empty;

                return feeling switch
                {
                    "sad" => "☹️",
                    "happy" => "😄",
                    "angry" => "😠",
                    "funny" => "😂",
                    "loved" => "😍",
                    "cool" => "🕶️",
                    "tired" => "😩",
                    "sleepy" => "😴",
                    "expressionless" => "😑",
                    "confused" => "😕",
                    "shocked" => "😱",
                    "so_sad" => "😭",
                    "blessed" => "😇",
                    "bored" => "😒",
                    "broken" => "💔",
                    "broke" => "💔",
                    "lovely" => "❤️",
                    "hot" => "😏",
                    _ => string.Empty
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return string.Empty;
            }
        }

        public static string GetSmileTypeIcon(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;

                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    {"smile", "😀"},
                    {"joy", "😂"},
                    {"relaxed", "😚"},
                    {"stuck-out-tongue-winking-eye", "😛"},
                    {"stuck-out-tongue", ":😜"},
                    {"sunglasses", "😎"},
                    {"wink", "😉"},
                    {"grin", "😁"},
                    {"smirk", "😏"},
                    {"innocent", "😇"},
                    {"cry", ":😢"},
                    {"sob", ":😭"},
                    {"disappointed", "😞"},
                    {"kissing-heart", "😘"},
                    {"heart", "❤️"},
                    {"broken-heart", "💔"},
                    {"heart-eyes", "😍"},
                    {"star", "⭐"},
                    {"open-mouth", "😦"},
                    {"scream", "😱"},
                    {"anguished", "😨"},
                    {"unamused", "😒"},
                    {"angry", "😡"},
                    {"rage", "😡"},
                    {"expressionless", "😑"},
                    {"confused", ":😕"},
                    {"neutral-face", "😐"},
                    {"exclamation", "❗"},
                    {"yum", "😋"},
                    {"triumph", "😤"},
                    {"imp", "😈"},
                    {"hear-no-evil", "🙉"},
                    {"alien", "👽"},
                    {"yellow-heart", "💛"},
                    {"sleeping", "😴"},
                    {"mask", "😷"},
                    {"no-mouth", "😈"},
                    {"weary", "😩"},
                    {"dizzy-face", "😵"},
                    {"man", "👨"},
                    {"woman", "👩"},
                    {"boy", "👦"},
                    {"girl", "👧"},
                    {"older-man", "👴"},
                    {"older-woman", "👵"},
                    {"cop", "👨‍✈️"},
                    {"dancers", "👯"},
                    {"speak-no-evil", "🙊"},
                    {"lips", "👄"},
                    {"see-no-evil", "🙈"},
                    {"dog", "🐕"},
                    {"bear", "🐻"},
                    {"rose", "🌹"},
                    {"gift-heart", "💝"},
                    {"ghost", "👻"},
                    {"bell", "🔔"},
                    {"video-game", "🎮"},
                    {"soccer", "⚽"},
                    {"books", "📚"},
                    {"moneybag", "💰"},
                    {"mortar-board", "🎓"},
                    {"hand", "🤚"},
                    {"tiger", "🐅"},
                    {"elephant", "🐘"},
                    {"scream-cat", "🙀"},
                    {"monkey", "🐒"},
                    {"bird", "🐦"},
                    {"snowflake", "❄️"},
                    {"sunny", "☀️"},
                    {"оcean", "🌊"},
                    {"umbrella", "☂️"},
                    {"hibiscus", "🌺"},
                    {"tulip", "🌷"},
                    {"computer", "💻"},
                    {"bomb", "💣"},
                    {"gem", "💎"},
                    {"ring", "💍"}
                };

                string pattern = @"(<i class=[""']twa-lg twa twa-(.*?)[""']>)";
                var aa = Regex.Matches(text, pattern);
                if (aa.Count > 0)
                {
                    foreach (var item in aa)
                    {
                        //<i class="twa-lg twa twa-joy">
                        var type = item.ToString().Split("twa-").Last().Replace(">", "").Replace('"', ' ').Replace(" ", "");

                        var containsValue = dictionary.ContainsKey(type);
                        if (containsValue)
                        {
                            var value = dictionary.FirstOrDefault(a => a.Key == type).Value;
                            text = text.Replace(item.ToString(), value);
                        }
                    }
                }

                return text;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public static string GetFeelingTypeTextString(string feeling, Context activityContext)
        {
            try
            {
                if (string.IsNullOrEmpty(feeling))
                    return string.Empty;

                return feeling switch
                {
                    "sad" => activityContext.GetText(Resource.String.Lbl_Sad),
                    "happy" => activityContext.GetText(Resource.String.Lbl_Happy),
                    "angry" => activityContext.GetText(Resource.String.Lbl_Angry),
                    "funny" => activityContext.GetText(Resource.String.Lbl_Funny),
                    "loved" => activityContext.GetText(Resource.String.Lbl_Loved),
                    "cool" => activityContext.GetText(Resource.String.Lbl_Cool),
                    "tired" => activityContext.GetText(Resource.String.Lbl_Tired),
                    "sleepy" => activityContext.GetText(Resource.String.Lbl_Sleepy),
                    "expressionless" => activityContext.GetText(Resource.String.Lbl_Expressionless),
                    "confused" => activityContext.GetText(Resource.String.Lbl_Confused),
                    "shocked" => activityContext.GetText(Resource.String.Lbl_Shocked),
                    "so_sad" => activityContext.GetText(Resource.String.Lbl_VerySad),
                    "blessed" => activityContext.GetText(Resource.String.Lbl_Blessed),
                    "bored" => activityContext.GetText(Resource.String.Lbl_Bored),
                    "broken" => activityContext.GetText(Resource.String.Lbl_Broken),
                    "lovely" => activityContext.GetText(Resource.String.Lbl_Lovely),
                    "hot" => activityContext.GetText(Resource.String.Lbl_Hot),
                    _ => string.Empty
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return string.Empty;
            }
        }

        public static bool GetVideosExtensions(string extenstion)
        {
            try
            {
                if (string.IsNullOrEmpty(extenstion))
                    return false;

                if (extenstion.Contains(".MP4") || extenstion.Contains(".mp4"))
                    return true;
                if (extenstion.Contains(".WMV") || extenstion.Contains(".wmv"))
                    return true;
                if (extenstion.Contains(".3GP") || extenstion.Contains(".3gp"))
                    return true;
                if (extenstion.Contains(".WEBM") || extenstion.Contains(".webm"))
                    return true;
                if (extenstion.Contains(".FLV") || extenstion.Contains(".flv"))
                    return true;
                if (extenstion.Contains(".AVI") || extenstion.Contains(".avi"))
                    return true;
                if (extenstion.Contains(".HDV") || extenstion.Contains(".hdv"))
                    return true;
                if (extenstion.Contains(".MPEG") || extenstion.Contains(".mpeg"))
                    return true;
                if (extenstion.Contains(".MXF") || extenstion.Contains(".mxf"))
                    return true;
                if (extenstion.Contains(".mov") || extenstion.Contains(".MOV"))
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool GetImagesExtensions(string extenstion)
        {
            try
            {
                if (string.IsNullOrEmpty(extenstion))
                    return false;

                if (extenstion.Contains(".PNG") || extenstion.Contains(".png"))
                    return true;
                if (extenstion.Contains(".JPG") || extenstion.Contains(".jpg"))
                    return true;
                if (extenstion.Contains(".GIF") || extenstion.Contains(".gif"))
                    return true;
                if (extenstion.Contains(".JPEG") || extenstion.Contains(".jpeg"))
                    return true;
                if (extenstion.Contains(".webp") || extenstion.Contains(".WEBP"))
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }
    }
}