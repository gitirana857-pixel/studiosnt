using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Content.Res;
using AndroidX.Core.Content;
using AndroidX.Core.Graphics.Drawable;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Load.Resource.Drawable;
using Bumptech.Glide.Request;
using Bumptech.Glide.Request.Target;
using Bumptech.Glide.Signature;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.Comment.Fragment;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using Exception = System.Exception;
using Math = System.Math;
using Object = Java.Lang.Object;
using Reaction = WoWonderClient.Classes.Posts.Reaction;
using String = Java.Lang.String;

namespace WoWonder.Activities.NativePost.Post
{
    public class AdapterBind
    {
        private readonly AppCompatActivity ActivityContext;
        private readonly NativePostAdapter NativePostAdapter;

        public AdapterBind(NativePostAdapter adapter)
        {
            try
            {
                ActivityContext = adapter.ActivityContext;
                NativePostAdapter = adapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PromotePostBind(AdapterHolders.PromoteHolder holder, AdapterModelsClass item)
        {
            try
            {
                bool isPromoted = item.PostData.IsPostBoosted == "1" || item.PostData.SharedInfo.SharedInfoClass != null && item.PostData.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                holder.PromoteLayout.Visibility = isPromoted switch
                {
                    false => ViewStates.Gone,
                    _ => holder.PromoteLayout.Visibility
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void HeaderPostBind(AdapterHolders.PostTopSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                UserDataObject publisher = item.PostData.Publisher ?? item.PostData.UserData;


                var glideRequestOptions2 = new RequestOptions().SkipMemoryCache(true).CenterCrop().CircleCrop().Format(DecodeFormat.PreferRgb565)
                    .SetPriority(Priority.High)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder_circle)
                    .Placeholder(Resource.Drawable.ImagePlacholder_circle);

                if (publisher.UserId == UserDetails.UserId)
                {
                    var CircleGlideRequestBuilder = Glide.With(holder.ItemView).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(item.PostData.PostPrivacy == "4" ? "user_anonymous" : UserDetails.Avatar).CircleCrop().Into(holder.UserAvatar);
                }
                else
                {
                    var CircleGlideRequestBuilder = Glide.With(holder.ItemView).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(item.PostData.PostPrivacy == "4" ? "user_anonymous" : publisher.Avatar).CircleCrop().Into(holder.UserAvatar);
                }

                //GlideImageLoader.LoadImage(ActivityContext, item.PostData.PostPrivacy == "4" ? "user_anonymous" : publisher.Avatar, holder.UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                switch (item.PostData.PostPrivacy)
                {
                    //Anonymous Post
                    case "4":
                        holder.Username.Text = ActivityContext.GetText(Resource.String.Lbl_Anonymous);
                        break;
                    default:
                        holder.Username.SetText(holder.Username.SetClick(holder.Username, item.PostData, item.PostDataDecoratedContent, holder));
                        break;
                }

                holder.TimeText.Text = item.PostData.Time;

                if (holder.PrivacyPostIcon != null && !string.IsNullOrEmpty(item.PostData.PostPrivacy) && (publisher.UserId == UserDetails.UserId || AppSettings.ShowPostPrivacyForAllUser))
                {
                    switch (item.PostData.PostPrivacy)
                    {
                        //Everyone
                        case "0":
                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_post_global_vector);
                            break;
                        default:
                            {
                                if (item.PostData.PostPrivacy.Contains("ifollow") || item.PostData.PostPrivacy == "2") //People_i_Follow
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_friend);
                                }
                                else if (item.PostData.PostPrivacy.Contains("me") || item.PostData.PostPrivacy == "1") //People_Follow_Me
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_users);
                                }
                                else switch (item.PostData.PostPrivacy)
                                    {
                                        //Anonymous
                                        case "4":
                                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_detective);
                                            break;
                                        //No_body 
                                        default:
                                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_lock);
                                            break;
                                    }

                                break;
                            }
                    }

                    holder.PrivacyPostIcon.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SharedHeaderPostBind(AdapterHolders.PostTopSharedSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                var itemPost = item.PostData;

                UserDataObject publisher = itemPost.Publisher ?? itemPost.UserData;

                var glideRequestOptions2 = new RequestOptions().SkipMemoryCache(true).CenterCrop().CircleCrop().Format(DecodeFormat.PreferRgb565)
                    .SetPriority(Priority.High)
                    .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                    .Error(Resource.Drawable.ImagePlacholder_circle)
                    .Placeholder(Resource.Drawable.ImagePlacholder_circle);

                if (publisher.UserId == UserDetails.UserId)
                {
                    var CircleGlideRequestBuilder = Glide.With(holder.ItemView).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(item.PostData.PostPrivacy == "4" ? "user_anonymous" : UserDetails.Avatar).CircleCrop().Into(holder.UserAvatar);
                }
                else
                {
                    var CircleGlideRequestBuilder = Glide.With(holder.ItemView).AsBitmap().Downsample(DownsampleStrategy.AtMost).Apply(glideRequestOptions2).Timeout(3000).SetUseAnimationPool(false);
                    CircleGlideRequestBuilder.DontTransform_T();
                    CircleGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside);
                    CircleGlideRequestBuilder.AddListener(new GlideCustomRequestListener("GlobalCircle")).Override(70).Load(itemPost.PostPrivacy == "4" ? "user_anonymous" : publisher.Avatar).CircleCrop().Into(holder.UserAvatar);
                }


                //switch (itemPost.PostPrivacy)
                //{
                //    case "4":
                //        GlideImageLoader.LoadImage(ActivityContext, "user_anonymous", holder.UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                //        break;
                //    default:
                //        NativePostAdapter.CircleGlideRequestBuilder.Load(publisher.Avatar).Into(holder.UserAvatar);
                //        break;
                //}

                holder.Username.Text = itemPost.PostPrivacy switch
                {
                    //Anonymous Post
                    "4" => ActivityContext.GetText(Resource.String.Lbl_Anonymous),
                    _ => WoWonderTools.GetNameFinal(publisher)
                };

                holder.TimeText.Text = itemPost.Time;

                if (holder.PrivacyPostIcon != null && !string.IsNullOrEmpty(itemPost.PostPrivacy) && (publisher.UserId == UserDetails.UserId || AppSettings.ShowPostPrivacyForAllUser))
                {
                    switch (itemPost.PostPrivacy)
                    {
                        //Everyone
                        case "0":
                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_post_global_vector);
                            break;
                        default:
                            {
                                if (itemPost.PostPrivacy.Contains("ifollow") || itemPost.PostPrivacy == "2") //People_i_Follow
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_friend);
                                }
                                else if (itemPost.PostPrivacy.Contains("me") || itemPost.PostPrivacy == "1") //People_Follow_Me
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_users);
                                }
                                else switch (itemPost.PostPrivacy)
                                    {
                                        //Anonymous
                                        case "4":
                                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_detective);
                                            break;
                                        //No_body) 
                                        default:
                                            holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.ic_lock);
                                            break;
                                    }

                                break;
                            }
                    }

                    holder.PrivacyPostIcon.Visibility = ViewStates.Visible;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PrevBottomPostPartBind(AdapterHolders.PostPrevBottomSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (holder.CommentCount != null)
                    holder.CommentCount.Text = item.PostData.PostComments + " " + ActivityContext.GetString(Resource.String.Lbl_Comments);

                if (AppSettings.ShowTextShareButton)
                {
                    if (holder.ShareCount != null)
                        holder.ShareCount.Text = item.PostData.DatumPostShare + " " + ActivityContext.GetString(Resource.String.Lbl_Shares);
                }
                else
                {
                    if (holder.ShareCount != null)
                        holder.ShareCount.Visibility = ViewStates.Gone;
                }

                holder.ViewCount.Text = item.PostData.PrevButtonViewText;

                if (holder.LikeCount != null)
                {
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Reaction:
                            holder.LikeCount.Text = item.PostData.PostLikes + " " + ActivityContext.GetString(Resource.String.Lbl_Reactions);
                            break;
                        default:
                            holder.LikeCount.Text = item.PostData.PostLikes + " " + ActivityContext.GetString(Resource.String.Btn_Likes);
                            break;
                    }
                }

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            item.PostData.Reaction ??= new Reaction();

                            holder.ImageCountLike.Visibility = item.PostData.Reaction.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
                            if (item.PostData.Reaction.Count > 0)
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                            else
                                holder.ImageCountLike.SetImageResource(Resource.Drawable.icon_post_like_vector);

                            if (item.PostData.Reaction.IsReacted != null && item.PostData.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(item.PostData.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == item.PostData.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    switch (item.PostData.Reaction.Count)
                                                    {
                                                        case > 0:
                                                            holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                            break;
                                                    }
                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                switch (item.PostData.Reaction.Count)
                                {
                                    case > 0:
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                }
                            }

                            break;
                        }
                    default:
                        //holder.ImageCountLike.Visibility = ViewStates.Invisible;
                        holder.ImageCountLike.SetImageResource(Resource.Drawable.icon_post_like_vector);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BottomPostPartBind(AdapterHolders.PostBottomSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (holder.LikeButton != null)
                    holder.LikeButton.Text = item.PostData.PostLikes;

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            item.PostData.Reaction ??= new Reaction();

                            if (item.PostData.Reaction.IsReacted != null && item.PostData.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(item.PostData.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == item.PostData.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Like);
                                                    break;
                                                case "2":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Love);
                                                    break;
                                                case "3":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                    break;
                                                case "4":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Wow);
                                                    break;
                                                case "5":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Sad);
                                                    break;
                                                case "6":
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Angry);
                                                    break;
                                                default:
                                                    holder.LikeButton.SetReactionPack(ReactConstants.Default);
                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                                holder.LikeButton.SetReactionPack(ReactConstants.Default);

                            break;
                        }
                    default:
                        {
                            if (item.PostData.Reaction.IsReacted != null && !item.PostData.Reaction.IsReacted.Value)
                                holder.LikeButton.SetReactionPack(ReactConstants.Default);

                            if (item.PostData.IsLiked != null && item.PostData.IsLiked.Value)
                                holder.LikeButton.SetReactionPack(ReactConstants.Like);

                            if (holder.SecondReactionButton != null)
                            {
                                switch (AppSettings.PostButton)
                                {
                                    case PostButtonSystem.Wonder when item.PostData.IsWondered != null && item.PostData.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_post_wonder_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Lbl_wondered);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                            break;
                                        }
                                    case PostButtonSystem.Wonder:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_post_wonder_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }
                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Btn_Wonder);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                            break;
                                        }
                                    case PostButtonSystem.DisLike when item.PostData.IsWondered != null && item.PostData.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_post_dislike_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Lbl_disliked);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                            break;
                                        }
                                    case PostButtonSystem.DisLike:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.icon_post_dislike_vector);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            switch (Build.VERSION.SdkInt)
                                            {
                                                case <= BuildVersionCodes.Lollipop:
                                                    DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                                    break;
                                                default:
                                                    wrappedDrawable = wrappedDrawable.Mutate();
                                                    wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                                    break;
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Btn_Dislike);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                            break;
                                        }
                                }
                            }

                            break;
                        }
                }

                var collection = item?.PostData?.SharedInfo.SharedInfoClass;
                if (item.IsSharingPost && collection != null)
                {
                    holder.ShareLinearLayout.Visibility = ViewStates.Gone;
                    holder.MainSectionButton.WeightSum = 2;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void TextSectionPostPartBind(AdapterHolders.PostTextSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (string.IsNullOrEmpty(item.PostData.Orginaltext) || string.IsNullOrWhiteSpace(item.PostData.Orginaltext))
                {
                    if (holder.Description.Visibility != ViewStates.Gone)
                        holder.Description.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (holder.Description.Visibility != ViewStates.Visible)
                        holder.Description.Visibility = ViewStates.Visible;

                    if (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                    {
                        holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, item.PostData.RegexFilterList);
                    }
                    else
                        holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());

                    var spendable = new SpannableStringBuilder(item.PostData.Orginaltext.Replace("@", ""));
                    if (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                    {
                        foreach (var user in item.PostData.RegexFilterList)
                        {
                            string fullName = item.PostData.MentionsUsers.MentionsUsersList?.FirstOrDefault(a => a.Key == user.Value?.Replace("/", "")).Value;

                            string content = spendable.ToString();
                            if (string.IsNullOrEmpty(content) || string.IsNullOrWhiteSpace(content))
                                continue;

                            var indexFrom = content.IndexOf(fullName, StringComparison.Ordinal);
                            indexFrom = indexFrom switch
                            {
                                <= -1 => 0,
                                _ => indexFrom
                            };

                            var indexLast = indexFrom + fullName.Length;
                            indexLast = indexLast switch
                            {
                                <= -1 => 0,
                                _ => indexLast
                            };

                            Console.WriteLine(indexFrom);

                            if (indexFrom == 0 && indexLast == 0)
                                continue;

                            spendable.SetSpan(new PostTextMentionsClickSpanClass(user.Key, NativePostAdapter.ActivityContext), indexFrom, indexLast, SpanTypes.ExclusiveExclusive);
                        }
                    }

                    NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.Description, spendable);

                    holder.Description.LinksClickable = true;
                    holder.Description.Clickable = true;
                    holder.Description.MovementMethod = LinkMovementMethod.Instance;

                    if (AppSettings.TextSizeDescriptionPost == WRecyclerView.VolumeState.On)
                    {
                        if (!string.IsNullOrEmpty(holder.Description.Text) && !string.IsNullOrWhiteSpace(holder.Description.Text) && holder.Description.Text?.Length <= 50)
                        {
                            if (item.PostData.Orginaltext.Contains("http") || item.PostData.Orginaltext.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadMore)) || item.PostData.Orginaltext.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                                holder.Description.SetTextSize(ComplexUnitType.Sp, 13f);
                            else
                                holder.Description.SetTextSize(ComplexUnitType.Sp, 20f);
                        }
                        else
                        {
                            holder.Description.SetTextSize(ComplexUnitType.Sp, 13f);
                        }
                    }
                    else
                    {
                        holder.Description.SetTextSize(ComplexUnitType.Sp, 13f);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public class PostTextMentionsClickSpanClass : ClickableSpan
        {
            private readonly AppCompatActivity ActivityContext;
            private readonly string UserId;

            public PostTextMentionsClickSpanClass(string userId, AppCompatActivity context)
            {
                try
                {
                    UserId = userId;
                    ActivityContext = context;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnClick(View widget)
            {
                try
                {
                    WoWonderTools.OpenProfile(ActivityContext, UserId, null);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void UpdateDrawState(TextPaint ds)
            {
                try
                {
                    base.UpdateDrawState(ds);
                    ds.Color = Color.ParseColor(AppSettings.MainColor);
                    ds.BgColor = Color.Transparent;
                    ds.UnderlineText = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

            }
        }

        public void CommentSectionBind(CommentAdapterViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                var comment = item?.PostData?.GetPostComments?.FirstOrDefault(danjo => string.IsNullOrEmpty(danjo?.CFile) && string.IsNullOrEmpty(danjo?.Record) && !string.IsNullOrEmpty(danjo?.Text));
                switch (comment)
                {
                    case null:
                        return;
                }

                var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(comment);
                LoadCommentData(db, holder);

                holder.CommentAdapter.CommentList = new ObservableCollection<CommentObjectExtra>(ClassMapper.Mapper?.Map<ObservableCollection<CommentObjectExtra>>(item.PostData.GetPostComments) ?? new ObservableCollection<CommentObjectExtra>());

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadCommentData(CommentObjectExtra item, RecyclerView.ViewHolder viewHolder)
        {
            try
            {
                if (viewHolder is not CommentAdapterViewHolder holder || item == null)
                    return;

                if (!string.IsNullOrEmpty(item.Orginaltext) || !string.IsNullOrWhiteSpace(item.Orginaltext))
                {
                    var text = Methods.FunString.DecodeString(item.Orginaltext);
                    holder.CommentText.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());
                    NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.CommentText, new String(text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }

                holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time), true);

                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                holder.UserName.Text = WoWonderTools.GetNameFinal(item.Publisher);

                if (AppSettings.FlowDirectionRightToLeft)
                    holder.UserName.SetCompoundDrawablesWithIntrinsicBounds(item.Publisher.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0, 0, 0);
                else
                    holder.UserName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, item.Publisher.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                //Image
                if (holder.ItemViewType == 1 || holder.CommentImage != null)
                {
                    //if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    //{
                    //    File file2 = new File(item.CFile);
                    //    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                    //    Glide.With(ActivityContext?.BaseContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);

                    //    //GlideImageLoader.LoadImage(ActivityContext,item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    //}
                    //else
                    //{
                    //    if (!item.CFile.Contains(InitializeWoWonder.WebsiteUrl))
                    //        item.CFile = WoWonderTools.GetTheFinalLink(item.CFile);

                    //    GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    //}
                }

                //Voice
                if (holder.VoiceLayout != null && !string.IsNullOrEmpty(item.Record))
                {
                    //LoadAudioItem(holder, position, item);
                }

                var repliesCount = !string.IsNullOrEmpty(item.RepliesCount) ? item.RepliesCount : item.Replies ?? "";
                if (repliesCount != "0" && !string.IsNullOrEmpty(repliesCount))
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + repliesCount + ")";

                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Reaction:
                        {
                            item.Reaction ??= new Reaction();

                            switch (item.Reaction.Count)
                            {
                                case > 0:
                                    holder.CountLikeSection.Visibility = ViewStates.Visible;
                                    holder.CountLike.Text = Methods.FunString.FormatPriceValue(item.Reaction.Count);
                                    break;
                                default:
                                    holder.CountLikeSection.Visibility = ViewStates.Gone;
                                    break;
                            }

                            if (item.Reaction.IsReacted != null && item.Reaction.IsReacted.Value)
                            {
                                switch (string.IsNullOrEmpty(item.Reaction.Type))
                                {
                                    case false:
                                        {
                                            var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == item.Reaction.Type).Value?.Id ?? "";
                                            switch (react)
                                            {
                                                case "1":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.Like);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                    break;
                                                case "2":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.Love);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_love);
                                                    break;
                                                case "3":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.HaHa);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_haha);
                                                    break;
                                                case "4":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.Wow);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_wow);
                                                    break;
                                                case "5":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.Sad);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_sad);
                                                    break;
                                                case "6":
                                                    ReactionComment.SetReactionPack(holder, ReactConstants.Angry);
                                                    holder.LikeTextView.Tag = "Liked";
                                                    holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_angry);
                                                    break;
                                                default:
                                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                                    //holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                                                    holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.ParseColor("#888888"));
                                                    holder.LikeTextView.Tag = "Like";

                                                    switch (item.Reaction.Count)
                                                    {
                                                        case > 0:
                                                            holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                                            break;
                                                    }
                                                    break;
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                //holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                                holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.ParseColor("#888888"));
                                holder.LikeTextView.Tag = "Like";

                                switch (item.Reaction.Count)
                                {
                                    case > 0:
                                        holder.ImageCountLike.SetImageResource(Resource.Drawable.emoji_like);
                                        break;
                                }
                            }

                            break;
                        }
                    default:
                        {
                            switch (item.IsCommentLiked)
                            {
                                case true:
                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                                    holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                default:
                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                    //holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                                    holder.LikeTextView.SetTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.ParseColor("#888888"));
                                    holder.LikeTextView.Tag = "Like";
                                    break;
                            }

                            break;
                        }
                }

                holder.TimeTextView.Tag = "true";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadImage(string path, ImageView image, [CallerMemberName] string memberName = "")
        {
            try
            {
                var FullGlideRequestBuilder = Glide.With(ActivityContext?.BaseContext).Load(path).Thumbnail(Glide.With(ActivityContext?.BaseContext).Load(path)
                    .Apply(RequestOptions.SignatureOf(new ObjectKey(path + "Thumb"))).SetSizeMultiplier(0.1f).SetPriority(Priority.Immediate)
                    .Downsample(DownsampleStrategy.CenterInside).CenterCrop().Override(50).AddListener(new GlideCustomRequestListener("Preload Half size " + memberName)))
                    .SetSizeMultiplier(0.95f).Apply(NativePostAdapter.GlideNormalOptions).Timeout(3000);

                FullGlideRequestBuilder.DontTransform_T();
                FullGlideRequestBuilder.Downsample(DownsampleStrategy.FitCenter).Transition(DrawableTransitionOptions.WithCrossFade(250));

                FullGlideRequestBuilder.Apply(RequestOptions.SignatureOf(new ObjectKey(path))).AddListener(new GlideCustomRequestListener("Preload Half size " + memberName)).Into(image);
            }
            catch (Exception e)
            {
                Console.WriteLine("LoadImage : " + memberName);
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MapPostBind(AdapterHolders.PostMapSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                Glide.With(ActivityContext?.BaseContext).Load(item.PostData.ImageUrlMap).Fallback(Resource.Drawable.Image_Map).Error(Resource.Drawable.Image_Map).Into(holder.Image);

                holder.MapTitle.Text = item.PostData.PostMap.Replace("/", " ");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ImagePostBind(AdapterHolders.PostImageSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                string imageUrl;
                switch (item.PostData.PhotoAlbum?.Count)
                {
                    case > 0:
                        {
                            var imagesList = item.PostData.PhotoAlbum;
                            imageUrl = imagesList[0].Image;
                            break;
                        }
                    default:
                        imageUrl = !string.IsNullOrEmpty(item.PostData.PostSticker) ? item.PostData.PostSticker : item.PostData.PostFileFull;
                        break;
                }
                holder.Image.Layout(0, 0, 0, 0);

                if (imageUrl.Contains(".gif"))
                    Glide.With(ActivityContext?.BaseContext).Load(imageUrl).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder)).Into(holder.Image);
                else
                    LoadImage(imageUrl, holder.Image);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MultiImageBind(AdapterHolders.PostMultiImageViewHolder holder, int count, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.PhotoMulti?.Count > 0 || item.PostData.PhotoAlbum?.Count > 0)
                {
                    var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                    switch (count)
                    {
                        case 2:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            break;
                        case 3:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            break;
                        case 4:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            break;
                        case 5:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            break;
                        case 6:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            LoadImage(imagesList[5].Image, holder.Image6);
                            break;
                        case 7:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            LoadImage(imagesList[5].Image, holder.Image6);
                            LoadImage(imagesList[6].Image, holder.Image7);
                            break;
                        case 8:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            LoadImage(imagesList[5].Image, holder.Image6);
                            LoadImage(imagesList[6].Image, holder.Image7);
                            LoadImage(imagesList[7].Image, holder.Image8);
                            break;
                        case 9:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            LoadImage(imagesList[5].Image, holder.Image6);
                            LoadImage(imagesList[6].Image, holder.Image7);
                            LoadImage(imagesList[7].Image, holder.Image8);
                            LoadImage(imagesList[8].Image, holder.Image9);
                            break;
                        case 10:
                            LoadImage(imagesList[0].Image, holder.Image);
                            LoadImage(imagesList[1].Image, holder.Image2);
                            LoadImage(imagesList[2].Image, holder.Image3);
                            LoadImage(imagesList[3].Image, holder.Image4);
                            LoadImage(imagesList[4].Image, holder.Image5);
                            LoadImage(imagesList[5].Image, holder.Image6);
                            LoadImage(imagesList[6].Image, holder.Image7);
                            LoadImage(imagesList[7].Image, holder.Image8);
                            LoadImage(imagesList[8].Image, holder.Image9);
                            LoadImage(imagesList[9].Image, holder.Image10);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MultiImagesBind(AdapterHolders.PostMultiImagesViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.PhotoMulti?.Count > 4 || item.PostData.PhotoAlbum?.Count > 4)
                {
                    var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                    LoadImage(imagesList[0].Image, holder.Image);
                    LoadImage(imagesList[1].Image, holder.Image2);
                    LoadImage(imagesList[2].Image, holder.Image3);
                    LoadImage(imagesList[3].Image, holder.Image4);

                    if (imagesList?.Count >= 5)
                    {
                        LoadImage(imagesList[4].Image, holder.Image5);
                        if (imagesList?.Count == 5)
                        {
                            holder.CountImageLabel.Visibility = ViewStates.Gone;
                            holder.Image6.Visibility = ViewStates.Gone;
                            holder.Image7.Visibility = ViewStates.Gone;
                            holder.ViewImage7.Visibility = ViewStates.Gone;
                            return;
                        }
                        LoadImage(imagesList[5].Image, holder.Image6);
                        if (imagesList?.Count == 6)
                        {
                            holder.CountImageLabel.Visibility = ViewStates.Gone;
                            holder.Image7.Visibility = ViewStates.Gone;
                            holder.ViewImage7.Visibility = ViewStates.Gone;
                            return;
                        }
                        LoadImage(imagesList[6].Image, holder.Image7);
                        if (imagesList?.Count == 7)
                        {
                            holder.CountImageLabel.Visibility = ViewStates.Gone;
                            holder.ViewImage7.Visibility = ViewStates.Gone;
                            return;
                        }
                        holder.CountImageLabel.Text = "+" + (imagesList?.Count - 7);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void VideoPostBind(AdapterHolders.PostVideoSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.VideoImage.Layout(0, 0, 0, 0);
                //holder.ExoPlayer.Layout(0, 0, 0, 0);
                // holder.MediaContainer.Layout(0, 0, 0, 0);

                var imageUrl = "";
                switch (string.IsNullOrEmpty(item.PostData.PostFileThumb))
                {
                    case false:
                        imageUrl = item.PostData.PostFileThumb;
                        break;
                    default:
                        imageUrl = item.PostData.PostFileFull;
                        break;
                }

                var FullGlideRequestBuilder = Glide.With(holder.ItemView).Load(imageUrl).Thumbnail(Glide.With(holder.ItemView).Load(imageUrl).Apply(RequestOptions.SignatureOf(new ObjectKey(imageUrl + "VideoThumb"))).SetSizeMultiplier(0.1f).SetPriority(Priority.Immediate).Downsample(DownsampleStrategy.CenterInside).Override(50).AddListener(new GlideCustomRequestListener("AdapterBind Thumbnail"))).SetSizeMultiplier(0.95f).Apply(NativePostAdapter.GlideNormalOptions).Timeout(3000);
                FullGlideRequestBuilder.DontTransform_T();
                FullGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside).Transition(DrawableTransitionOptions.WithCrossFade(250));
                FullGlideRequestBuilder.Apply(RequestOptions.SignatureOf(new ObjectKey(imageUrl))).Override(NativePostAdapter.ScreenWidthPixels, NativePostAdapter.ScreenHeightPixels).AddListener(new GlideCustomRequestListener("AdapterBind Video Normal")).Into(holder.VideoImage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BlogPostBind(AdapterHolders.PostBlogSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (string.IsNullOrEmpty(item.PostData?.Blog?.BlogClass.Thumbnail))
                {
                    case false:
                        LoadImage(item.PostData?.Blog?.BlogClass.Thumbnail, holder.ImageBlog);
                        break;
                }

                holder.PostBlogText.Text = item.PostData?.Blog?.BlogClass.Title;
                //holder.PostBlogContent.Text = item.PostData?.Blog.Description;

                CategoriesController cat = new CategoriesController();
                string id = item.PostData?.Blog?.BlogClass.CategoryLink.Split('/').Last();
                holder.CatText.Text = cat.Get_Translate_Categories_Communities(id, item.PostData?.Blog?.BlogClass.CategoryName, "Blog");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ColorPostBind(AdapterHolders.PostColorBoxSectionViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (string.IsNullOrEmpty(item.PostData.ColorBoxImageUrl))
                {
                    case false:
                        NativePostAdapter.FullGlideRequestBuilder.Load(item.PostData.ColorBoxImageUrl).Into(holder.ColorBoxImage);
                        break;
                }

                if (item.PostData.ColorBoxGradientDrawable != null)
                    holder.ColorBoxImage.Background = item.PostData.ColorBoxGradientDrawable;

                if (item.PostData != null)
                {
                    holder.DesTextView.SetTextColor(Color.ParseColor(item.PostData.ColorBoxTextColor));

                    switch (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                    {
                        case true:
                            holder.DesTextView.SetAutoLinkOnClickListener(NativePostAdapter, item.PostData.RegexFilterList);
                            break;
                        default:
                            holder.DesTextView.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());
                            break;
                    }

                    NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.DesTextView, new String(item.PostData.Orginaltext));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void EventPostBind(AdapterHolders.EventPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.Event?.EventClass != null)
                {
                    LoadImage(item.PostData?.Event?.EventClass.Cover, holder.Image);
                    holder.TxtEventTitle.Text = item.PostData?.Event?.EventClass?.Name;
                    //holder.TxtEventDescription.Text = item.PostData?.Event?.EventClass?.Description;
                    holder.TxtEventLocation.Text = item.PostData?.Event?.EventClass?.Location;
                    holder.TxtEventTime.Text = item.PostData?.Event?.EventClass?.EndDate;
                    holder.TxtEventGoing.Text = item.PostData?.Event?.EventClass?.GoingCount + " " + ActivityContext.GetText(Resource.String.Lbl_GoingPeople);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LinkPostBind(AdapterHolders.LinkPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.LinkUrl.Text = item.PostData?.PostLink;

                switch (string.IsNullOrEmpty(item.PostData?.PostLinkTitle))
                {
                    case false:
                        holder.PostLinkTitle.Text = item.PostData?.PostLinkTitle;
                        break;
                    default:
                        holder.PostLinkTitle.Visibility = ViewStates.Gone;
                        break;
                }

                //switch (string.IsNullOrEmpty(item.PostData?.PostLinkContent))
                //{
                //    case false:
                //        holder.PostLinkContent.Text = item.PostData?.PostLinkContent;
                //        break;
                //    default:
                //        holder.PostLinkContent.Visibility = ViewStates.Gone;
                //        break;
                //}

                if (string.IsNullOrEmpty(item.PostData?.PostLinkImage) || item.PostData.PostLinkTitle.Contains("Page Not Found") || item.PostData.PostLinkContent.Contains("See posts, photos and more on Facebook."))
                    holder.Image.Visibility = ViewStates.Gone;
                else
                {
                    var loader = Glide.With(holder.ItemView).Load(item.PostData?.PostLinkImage).Thumbnail(Glide.With(holder.ItemView).Load(item.PostData?.PostLinkImage)
                        .Apply(RequestOptions.SignatureOf(new ObjectKey(item.PostData?.PostLinkImage + "VideoThumb"))).SetSizeMultiplier(0.1f).SetPriority(Priority.Immediate)
                        .Downsample(DownsampleStrategy.CenterInside).Override(50)).SetSizeMultiplier(0.95f).Apply(NativePostAdapter.GlideNormalOptions).Timeout(3000);
                    loader.DontTransform_T();
                    loader.Downsample(DownsampleStrategy.CenterInside).Transition(DrawableTransitionOptions.WithCrossFade(250));
                    loader.Apply(RequestOptions.SignatureOf(new ObjectKey(item.PostData?.PostLinkImage))).Override(NativePostAdapter.ScreenWidthPixels, NativePostAdapter.ScreenHeightPixels);

                    if (item.PostData.PostLink.Contains("facebook.com") || item.PostData.PostLinkImage.Contains("facebook.png"))
                        loader.Load(item.PostData.PostLinkImage).Error(Resource.Drawable.facebook).Placeholder(Resource.Drawable.facebook);
                    else if (item.PostData.PostLink.Contains("vimeo.com") || item.PostData.PostLinkImage.Contains("vimeo.png"))
                        loader.Load(Resource.Drawable.vimeo).Error(Resource.Drawable.vimeo).Placeholder(Resource.Drawable.vimeo);
                    else if (item.PostData.PostLinkImage.Contains("default_video_thumbnail.png"))
                        loader.Load(Resource.Drawable.default_video_thumbnail).Error(Resource.Drawable.default_video_thumbnail).Placeholder(Resource.Drawable.default_video_thumbnail);
                    else
                        loader.Load(item.PostData.PostLinkImage).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));

                    loader.Into(holder.Image);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FundingPostBind(AdapterHolders.FundingPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.FundData != null)
                {
                    LoadImage(item.PostData.FundData.Value.FundDataClass.Image, holder.Image);

                    holder.Title.Text = item.PostData.FundData.Value.FundDataClass.Title;
                    holder.Raised.Text = item.PostData.FundData.Value.FundDataClass.Raised;
                    holder.TottalAmount.Text = item.PostData.FundData.Value.FundDataClass.Amount;
                    holder.Progress.Progress = Convert.ToInt32(item.PostData.FundData.Value.FundDataClass.Bar);

                    item.PostData.FundData.Value.FundDataClass.UserData ??= item.PostData.Publisher;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PurpleFundPostBind(AdapterHolders.FundingPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData?.Fund?.PurpleFund?.Fund != null)
                {
                    LoadImage(item.PostData?.Fund?.PurpleFund?.Fund.Image, holder.Image);

                    holder.Title.Text = item.PostData?.Fund?.PurpleFund?.Fund.Title;
                    holder.Raised.Text = item.PostData?.Fund?.PurpleFund?.Fund.Raised;
                    holder.TottalAmount.Text = item.PostData?.Fund?.PurpleFund?.Fund.Amount;
                    holder.Progress.Progress = Convert.ToInt32(item.PostData?.Fund?.PurpleFund?.Fund.Bar);

                    item.PostData.Fund.Value.PurpleFund.Fund.UserData ??= item.PostData.Publisher;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ProductPostBind(AdapterHolders.ProductPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.Product != null)
                {
                    switch (item.PostData.Product.Value.ProductClass?.Images.Count)
                    {
                        case > 0:
                            LoadImage(item.PostData.Product.Value.ProductClass?.Images[0].Image, holder.Image);
                            break;
                    }

                    switch (item.PostData.Product.Value.ProductClass?.Seller)
                    {
                        case null:
                            {
                                if (item.PostData.Product.Value.ProductClass != null)
                                    item.PostData.Product.Value.ProductClass.Seller = item.PostData.Publisher;
                                break;
                            }
                    }

                    //switch (string.IsNullOrEmpty(item.PostData.Product.Value.ProductClass?.LocationDecodedText))
                    //{
                    //    case false:
                    //        holder.PostProductLocationText.Text = item.PostData.Product.Value.ProductClass?.LocationDecodedText;
                    //        break;
                    //    default:
                    //        holder.PostProductLocationText.Visibility = ViewStates.Gone;
                    //        break;
                    //}

                    holder.PostLinkTitle.Text = item.PostData.Product.Value.ProductClass?.Name;
                    holder.PostProductContent.Text = Methods.FunString.SubStringCutOf(item.PostData.Product.Value.ProductClass?.Description, 100);
                    //holder.PriceText.Text = item.PostData.Product.Value.ProductClass?.CurrencyText;
                    //holder.TypeText.Text = item.PostData.Product.Value.ProductClass?.TypeDecodedText;
                    //holder.StatusText.Text = item.PostData.Product.Value.ProductClass?.StatusDecodedText;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void VoicePostBind(AdapterHolders.SoundPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AgoraLivePostBind(AdapterHolders.PostAgoraLiveViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.LiveTime != null && item.PostData.LiveTime.Value > 0)
                {
                    holder.TxtName.Text = WoWonderTools.GetNameFinal(item.PostData.Publisher) + " " + ActivityContext.GetText(Resource.String.Lbl_StartedBroadcastingLive);
                }
                else
                {
                    holder.TxtName.Text = WoWonderTools.GetNameFinal(item.PostData.Publisher) + " " + ActivityContext.GetText(Resource.String.Lbl_StreamHasEnded);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void WebViewPostBind(AdapterHolders.PostPlayTubeContentViewHolder holder, AdapterModelsClass item, PostModelType itemViewType)
        {
            try
            {
                switch (itemViewType)
                {
                    case PostModelType.PlayTubePost:
                        {
                            var playTubeUrl = ListUtils.SettingsSiteList?.PlaytubeUrl;

                            var fullEmbedUrl = playTubeUrl + "/embed/" + item.PostData.PostPlaytube;

                            switch (AppSettings.EmbedPlayTubeVideoPostType)
                            {
                                case VideoPostTypeSystem.EmbedVideo:
                                    {
                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 600;
                                        holder.WebView.LayoutParameters = vc;

                                        holder.WebView.LoadUrl(fullEmbedUrl);
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }

                            break;
                        }
                    case PostModelType.LivePost:
                        {
                            var liveUrl = "https://viewer.millicast.com/v2?streamId=";
                            var id = ListUtils.SettingsSiteList?.LiveAccountId;
                            string fullEmbedUrl = liveUrl + id + "/" + item.PostData.StreamName;

                            switch (AppSettings.EmbedLivePostType)
                            {
                                case true:
                                    {
                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 600;
                                        holder.WebView.LayoutParameters = vc;

                                        holder.WebView.LoadUrl(fullEmbedUrl);
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }
                            break;
                        }
                    case PostModelType.DeepSoundPost:
                        {
                            var deepSoundUrl = ListUtils.SettingsSiteList?.DeepsoundUrl;

                            var fullEmbedUrl = deepSoundUrl + "/embed/" + item.PostData.PostDeepsound;

                            switch (AppSettings.EmbedDeepSoundPostType)
                            {
                                case true:
                                    {
                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 480;
                                        holder.WebView.LayoutParameters = vc;

                                        holder.WebView.LoadUrl(fullEmbedUrl);
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }

                            break;
                        }
                    case PostModelType.VimeoPost:
                        {
                            var fullEmbedUrl = "https://player.vimeo.com/video/" + item.PostData.PostVimeo;

                            switch (AppSettings.EmbedVimeoVideoPostType)
                            {
                                case VideoPostTypeSystem.EmbedVideo:
                                    {
                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 700;
                                        holder.WebView.LayoutParameters = vc;

                                        holder.WebView.LoadUrl(fullEmbedUrl);
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }

                            break;
                        }
                    case PostModelType.FacebookPost:
                        {
                            //var content = "<iframe src='https://www.facebook.com/plugins/video.php?height=600&href=https://www.facebook.com/" + item.PostData.PostFacebook + "/&show_text=false'";
                            string content;

                            if (item.PostData.PostFacebook.Contains("https://fb.watch"))
                                content = "<iframe src='https://www.facebook.com/plugins/video.php?href=" + item.PostData.PostFacebook + "&show_text=false&appId=" + ActivityContext.GetText(Resource.String.facebook_app_id) + "&height=600&show_text=false'";
                            else if (item.PostData.PostFacebook.Contains("https://www.facebook.com"))
                                content = "<iframe src='https://www.facebook.com/plugins/video.php?href=" + item.PostData.PostFacebook + "&show_text=false&appId=" + ActivityContext.GetText(Resource.String.facebook_app_id) + "&height=600&show_text=false'";
                            else
                                content = "<iframe src='https://www.facebook.com/plugins/video.php?href=https://fb.watch/" + item.PostData.PostFacebook + "&show_text=false&appId=" + ActivityContext.GetText(Resource.String.facebook_app_id) + "&height=600&show_text=false'";

                            content += "width='100%' height='600' style='border:none;overflow:hidden'";
                            content += "scrolling='no' frameborder='0' allowfullscreen='true'";
                            content += "allow='autoplay; clipboard-write; encrypted-media; picture-in-picture; web-share'";
                            content += "allowFullScreen='true'>";
                            content += "</iframe>";

                            var dataWebHtml = "<!DOCTYPE html>";
                            dataWebHtml += "<head><title></title>" + "</head>";
                            dataWebHtml += "<body>" + content + "</body>";
                            dataWebHtml += "</html>";

                            var fullEmbedUrl = "https://www.facebook.com/video/embed?video_id=" + item.PostData.PostFacebook.Split("/videos/").Last();
                            switch (AppSettings.EmbedFacebookVideoPostType)
                            {
                                case VideoPostTypeSystem.EmbedVideo:
                                    {
                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 600;
                                        holder.WebView.LayoutParameters = vc;

                                        //Load url to be rendered on WebView 
                                        //holder.WebView.LoadUrl(fullEmbedUrl);
                                        holder.WebView.LoadDataWithBaseURL(null, dataWebHtml, "text/html", "UTF-8", null);
                                        holder.WebView.Visibility = ViewStates.Visible;
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }

                            break;
                        }
                    case PostModelType.TikTokPost:
                        {
                            var fullEmbedUrl = item.PostData.PostLink;
                            switch (AppSettings.EmbedTikTokVideoPostType)
                            {
                                case VideoPostTypeSystem.EmbedVideo:
                                    {
                                        //var videoId = item.PostData.PostTikTok.Split("/video/").Last();
                                        //string content = "<blockquote class='tiktok-embed' cite='" + item.PostData.PostTikTok + "' data-video-id='" + videoId + "' style='max-width: 605px;min-width: 325px;'>" +
                                        //                 "<iframe src='" + item.PostData.PostTikTok + "' style='width: 100%; height: 863px; display: block; visibility: unset; max-height: 863px;'></iframe>" +
                                        //                 "</blockquote>" + "" +
                                        //                 "<script async='' src='https://www.tiktok.com/embed.js'></script>";

                                        //string DataWebHtml = "<!DOCTYPE html>";
                                        //DataWebHtml += "<head><title></title></head>";
                                        //DataWebHtml += "<body>" + content + "</body>";
                                        //DataWebHtml += "</html>";

                                        var vc = holder.WebView.LayoutParameters;
                                        vc.Height = 1200;
                                        holder.WebView.LayoutParameters = vc;

                                        //holder.WebView.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);
                                        holder.WebView.LoadUrl(item.PostData.PostTikTok);
                                        holder.WebView.Visibility = ViewStates.Visible;
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }
                            break;
                        }
                    case PostModelType.TwitterPost:
                        {
                            var fullEmbedUrl = item.PostData.PostLink;
                            switch (AppSettings.EmbedTwitterPostType)
                            {
                                case VideoPostTypeSystem.EmbedVideo:
                                    {
                                        if (string.IsNullOrEmpty(item.PostData.PostTwitterEmbad))
                                        {
                                            item.PostData.PostTwitterEmbad = await ApiRequest.ApiGetInfoTwitterEmbedAsync(item.PostData.PostTwitter);
                                        }

                                        string content = item.PostData.PostTwitterEmbad;
                                        string DataWebHtml = "<!DOCTYPE html>";
                                        DataWebHtml += "<head>" +
                                                       "<title></title>" +
                                                       "<meta charset='utf-8'>" +
                                                       "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                                                       "</head>";
                                        DataWebHtml += "<body>" + content + "</body>";
                                        DataWebHtml += "</html>";

                                        //var vc = holder.WebView.LayoutParameters;
                                        //vc.Height = 900;
                                        //holder.WebView.LayoutParameters = vc;

                                        holder.WebView.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);
                                        //holder.WebView.LoadUrl(fullEmbedUrl);
                                        holder.WebView.Visibility = ViewStates.Visible;
                                        break;
                                    }
                                default:
                                    item.PostData.PostLink = fullEmbedUrl;
                                    holder.WebView.Visibility = ViewStates.Gone;
                                    break;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OfferPostBind(AdapterHolders.OfferPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (string.IsNullOrEmpty(item.PostData.Offer?.OfferClass?.Image))
                {
                    case false:
                        LoadImage(item.PostData.Offer?.OfferClass?.Image, holder.ImageOffer);
                        break;
                }

                holder.OfferText.Text = Methods.FunString.SubStringCutOf(item.PostData.Offer?.OfferClass?.OfferText, 100);
                holder.OfferContent.Text = Methods.FunString.SubStringCutOf(item.PostData.Offer?.OfferClass?.Description, 100);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void JobPostSectionBind(AdapterHolders.JobPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PostData.Job != null)
                {

                    LoadImage(item.PostData.Job.Value.JobInfoClass.Image, holder.JobCoverImage);

                    if (item.PostData.Job.Value.JobInfoClass.Page != null)
                    {
                        holder.PageName.Text = item.PostData.Job.Value.JobInfoClass.Page.PageName;
                    }

                    string salary = "$" + item.PostData.Job.Value.JobInfoClass.Minimum + " to " + "$" + item.PostData.Job.Value.JobInfoClass.Maximum;
                    string jobtype = item.PostData.Job.Value.JobInfoClass.JobType switch
                    {
                        //Set job type
                        "full_time" => ActivityContext.GetString(Resource.String.Lbl_full_time),
                        "part_time" => ActivityContext.GetString(Resource.String.Lbl_part_time),
                        "internship" => ActivityContext.GetString(Resource.String.Lbl_internship),
                        "volunteer" => ActivityContext.GetString(Resource.String.Lbl_volunteer),
                        "contract" => ActivityContext.GetString(Resource.String.Lbl_contract),
                        _ => ActivityContext.GetString(Resource.String.Lbl_Unknown)

                    };

                    holder.JobInfo.Text = salary + " - " + jobtype;

                    holder.JobTitle.Text = item.PostData.Job.Value.JobInfoClass.Title;

                    if (item.PostData.Job.Value.JobInfoClass.ButtonText.Contains(ActivityContext.GetString(Resource.String.Lbl_show_applies)))
                    {
                        holder.JobButton.Tag = "ShowApply";
                    }
                    else if (item.PostData.Job.Value.JobInfoClass.ButtonText.Contains(ActivityContext.GetString(Resource.String.Lbl_already_applied)))
                    {
                        holder.JobButton.Enabled = false;
                    }
                    else if (item.PostData.Job.Value.JobInfoClass.ButtonText.Contains(ActivityContext.GetString(Resource.String.Lbl_apply_now)))
                    {
                        holder.JobButton.Tag = "Apply";
                    }

                    holder.JobButton.Text = item.PostData.Job?.JobInfoClass.ButtonText;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PollPostBind(AdapterHolders.PollsPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.VoteText.Text = item.PollsOption.Text;
                holder.ProgressBarView.Progress = Convert.ToInt32(item.PollsOption.PercentageNum);
                holder.ProgressText.Text = item.PollsOption.Percentage;

                switch (string.IsNullOrEmpty(item.PostData.VotedId))
                {
                    case false when item.PostData.VotedId != "0":
                        {
                            if (item.PollsOption.Id == item.PostData.VotedId)
                            {
                                holder.CheckIcon.SetImageResource(Resource.Drawable.icon_checkmark_filled_vector);
                                holder.CheckIcon.ClearColorFilter();
                                holder.ProgressText.SetTextColor(new Color(ContextCompat.GetColor(holder.VoteText.Context, Resource.Color.primary)));
                                holder.ProgressBarView.ProgressDrawable = ContextCompat.GetDrawable(holder.ProgressBarView.Context, Resource.Drawable.primary_progress);
                            }
                            else
                            {
                                holder.CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                                holder.CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                            }

                            break;
                        }
                    default:
                        holder.CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                        holder.CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AlertBoxBind(AdapterHolders.AlertAdapterViewHolder holder, AdapterModelsClass item, PostModelType itemViewType)
        {
            try
            {
                switch (itemViewType)
                {
                    case PostModelType.AlertBox:
                        {
                            holder.HeadText.Text = string.IsNullOrEmpty(item.AlertModel?.TitleHead) switch
                            {
                                false => item.AlertModel?.TitleHead,
                                _ => holder.HeadText.Text
                            };

                            holder.SubText.Text = string.IsNullOrEmpty(item.AlertModel?.SubText) switch
                            {
                                false => item.AlertModel?.SubText,
                                _ => holder.SubText.Text
                            };

                            if (item.AlertModel?.ImageDrawable != null)
                                holder.Image.SetImageResource(item.AlertModel.ImageDrawable);

                            switch (string.IsNullOrEmpty(item.AlertModel?.LinerColor))
                            {
                                case false:
                                    holder.LineView.SetBackgroundColor(Color.ParseColor(item.AlertModel?.LinerColor));
                                    break;
                            }
                            break;
                        }
                    case PostModelType.AlertBoxAnnouncement:
                        {
                            holder.HeadText.Text = string.IsNullOrEmpty(item.AlertModel?.TitleHead) switch
                            {
                                false => Methods.FunString.DecodeString(item.AlertModel?.TitleHead),
                                _ => holder.HeadText.Text
                            };

                            holder.SubText.Text = string.IsNullOrEmpty(item.AlertModel?.SubText) switch
                            {
                                false => Methods.FunString.DecodeString(item.AlertModel?.SubText),
                                _ => holder.SubText.Text
                            };
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AlertJoinBoxBind(AdapterHolders.AlertJoinAdapterViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.HeadText.Text = string.IsNullOrEmpty(item.AlertModel?.TitleHead) switch
                {
                    false => item.AlertModel?.TitleHead,
                    _ => holder.HeadText.Text
                };

                holder.SubText.Text = string.IsNullOrEmpty(item.AlertModel?.SubText) switch
                {
                    false => item.AlertModel?.SubText,
                    _ => holder.SubText.Text
                };

                if (item.AlertModel?.ImageDrawable != null)
                    holder.NormalImageView.SetImageResource(item.AlertModel.ImageDrawable);
                else
                    holder.NormalImageView.Visibility = ViewStates.Gone;

                if (item.AlertModel?.IconImage != null)
                    holder.IconImageView.SetImageResource(item.AlertModel.IconImage);

                switch (item.AlertModel?.TypeAlert)
                {
                    case "Groups":
                        holder.MainRelativeLayout.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Linear);
                        holder.ButtonView.Text = ActivityContext.GetString(Resource.String.Lbl_FindYourGroups);
                        break;
                    case "Pages":
                        holder.MainRelativeLayout.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Linear1);
                        holder.ButtonView.Text = ActivityContext.GetString(Resource.String.Lbl_FindPopularPages);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SocialLinksBind(AdapterHolders.SocialLinksViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Facebook) == "Empty")
                {
                    holder.BtnFacebook.Enabled = false;
                    holder.BtnFacebook.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnFacebook.Enabled = true;

                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Google) == "Empty")
                {
                    holder.BtnGoogle.Enabled = false;
                    holder.BtnGoogle.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnGoogle.Enabled = true;

                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Twitter) == "Empty")
                {
                    holder.BtnTwitter.Enabled = false;
                    holder.BtnTwitter.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnTwitter.Enabled = true;

                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Youtube) == "Empty")
                {
                    holder.BtnYoutube.Enabled = false;
                    holder.BtnYoutube.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnYoutube.Enabled = true;

                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Vk) == "Empty")
                {
                    holder.BtnVk.Enabled = false;
                    holder.BtnVk.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnVk.Enabled = true;

                if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Instegram) == "Empty")
                {
                    holder.BtnInstegram.Enabled = false;
                    holder.BtnInstegram.SetColor(Color.ParseColor("#8c8a8a"));
                }
                else
                    holder.BtnInstegram.Enabled = true;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void AboutBoxBind(AdapterHolders.AboutBoxViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.AboutHead.Text = string.IsNullOrEmpty(item.AboutModel.TitleHead) switch
                {
                    false => item.AboutModel.TitleHead,
                    _ => holder.AboutHead.Text
                };

                holder.AboutDescription.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());
                holder.AboutDescription.Text = Methods.FunString.DecodeString(item.AboutModel.Description);
                NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.AboutDescription, new String(holder.AboutDescription.Text));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InfoUserBoxBind(AdapterHolders.InfoUserBoxViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.InfoUserModel.UserData != null)
                {
                    if (item.InfoUserModel.UserData.IsPro == "1")
                    {
                        holder.LayoutPro.Visibility = ViewStates.Visible;

                        var type = ListUtils.SettingsSiteList?.ProPackages?.FirstOrDefault(pair => pair.Key == item.InfoUserModel.UserData.ProType).Value?.Type?.ToLower();
                        if (!string.IsNullOrEmpty(type))
                        {
                            if (type == "star")
                            {
                                holder.ProText.Text = ActivityContext.GetText(Resource.String.Lbl_Star) + " " + ActivityContext.GetText(Resource.String.Lbl_Member);
                            }
                            else if (type == "hot")
                            {
                                holder.ProText.Text = ActivityContext.GetText(Resource.String.Lbl_Hot) + " " + ActivityContext.GetText(Resource.String.Lbl_Member);
                            }
                            else if (type == "ultima")
                            {
                                holder.ProText.Text = ActivityContext.GetText(Resource.String.Lbl_Ultima) + " " + ActivityContext.GetText(Resource.String.Lbl_Member);
                            }
                            else if (type == "vip")
                            {
                                holder.ProText.Text = ActivityContext.GetText(Resource.String.Lbl_Vip) + " " + ActivityContext.GetText(Resource.String.Lbl_Member);
                            }
                            else
                            {
                                holder.ProText.Text = type.ToUpper() + " " + ActivityContext.GetText(Resource.String.Lbl_Member);
                            }
                        }
                        else
                            holder.ProText.Text = ActivityContext.GetText(Resource.String.Lbl_ProMember);
                    }
                    else
                    {
                        holder.LayoutPro.Visibility = ViewStates.Gone;
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.Website))
                    {
                        case false:
                            holder.WebsiteText.Text = item.InfoUserModel.UserData.Website;
                            holder.LayoutWebsite.Visibility = ViewStates.Visible;
                            break;
                        default:
                            holder.LayoutWebsite.Visibility = ViewStates.Gone;
                            break;
                    }

                    switch (ListUtils.SettingsSiteList?.Genders?.Count)
                    {
                        case > 0:
                            {
                                var value = ListUtils.SettingsSiteList?.Genders?.FirstOrDefault(a => a.Key == item.InfoUserModel.UserData.Gender).Value;
                                holder.GanderText.Text = value ?? item.InfoUserModel.UserData.GenderText;

                                break;
                            }
                        default:
                            {
                                if (item.InfoUserModel.UserData.Gender == ActivityContext.GetText(Resource.String.Radio_Male))
                                {
                                    holder.GanderText.Text = ActivityContext.GetText(Resource.String.Radio_Male);
                                }
                                else if (item.InfoUserModel.UserData.Gender == ActivityContext.GetText(Resource.String.Radio_Female))
                                {
                                    holder.GanderText.Text = ActivityContext.GetText(Resource.String.Radio_Female);
                                }
                                else
                                {
                                    holder.GanderText.Text = item.InfoUserModel.UserData.GenderText;
                                }

                                break;
                            }
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.Birthday))
                    {
                        case false when item.InfoUserModel.UserData.BirthPrivacy != "2" && item.InfoUserModel.UserData.Birthday != "00-00-0000" && item.InfoUserModel.UserData.Birthday != "0000-00-00":
                            try
                            {
                                DateTime date = DateTime.Parse(item.InfoUserModel.UserData.Birthday);
                                string newFormat = date.Day + "/" + date.Month + "/" + date.Year;
                                holder.BirthdayText.Text = newFormat;
                            }
                            catch
                            {
                                //Methods.DisplayReportResultTrack(e);
                                holder.BirthdayText.Text = item.InfoUserModel.UserData.Birthday;
                            }
                            holder.LayoutBirthday.Visibility = ViewStates.Visible;
                            break;
                        default:
                            holder.LayoutBirthday.Visibility = ViewStates.Gone;
                            break;
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.Working))
                    {
                        case false:
                            holder.WorkText.Text = ActivityContext.GetText(Resource.String.Lbl_WorkingAt) + " " + item.InfoUserModel.UserData.Working;
                            holder.LayoutWork.Visibility = ViewStates.Visible;
                            break;
                        default:
                            holder.LayoutWork.Visibility = ViewStates.Gone;
                            break;
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.CountryId))
                    {
                        case false when item.InfoUserModel.UserData.CountryId != "0":
                            {
                                var countryName = WoWonderTools.GetCountryList(ActivityContext).FirstOrDefault(a => a.Key == item.InfoUserModel.UserData.CountryId).Value;

                                holder.LiveText.Text = ActivityContext.GetText(Resource.String.Lbl_LivingIn) + " " + countryName;
                                holder.LayoutLive.Visibility = ViewStates.Visible;
                                break;
                            }
                        default:
                            holder.LayoutLive.Visibility = ViewStates.Gone;
                            break;
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.School))
                    {
                        case false:
                            holder.StudyText.Text = ActivityContext.GetText(Resource.String.Lbl_StudyingAt) + " " + item.InfoUserModel.UserData.School;
                            holder.LayoutStudy.Visibility = ViewStates.Visible;
                            break;
                        default:
                            holder.LayoutStudy.Visibility = ViewStates.Gone;
                            break;
                    }

                    switch (string.IsNullOrEmpty(item.InfoUserModel.UserData.RelationshipId))
                    {
                        case false when item.InfoUserModel.UserData.RelationshipId != "0":
                            {
                                string relationship = WoWonderTools.GetRelationship(Convert.ToInt32(item.InfoUserModel.UserData.RelationshipId));
                                if (Methods.FunString.StringNullRemover(relationship) != "Empty")
                                {
                                    holder.RelationshipText.Text = relationship;
                                    holder.LayoutRelationship.Visibility = ViewStates.Visible;
                                }
                                else
                                    holder.LayoutRelationship.Visibility = ViewStates.Gone;

                                break;
                            }
                        default:
                            holder.LayoutRelationship.Visibility = ViewStates.Gone;
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InfoGroupBoxBind(AdapterHolders.InfoGroupBoxViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PrivacyModelClass?.GroupClass != null)
                {
                    holder.CategoryText.Text = Methods.FunString.DecodeString(item.PrivacyModelClass?.GroupClass.Category);
                    holder.PrivacyText.Text = ActivityContext.GetText(item.PrivacyModelClass?.GroupClass.Privacy == "1" ? Resource.String.Radio_Public : Resource.String.Radio_Private);
                    holder.IconPrivacy.SetImageResource(item.PrivacyModelClass?.GroupClass.Privacy == "1" ? Resource.Drawable.icon_post_global_vector : Resource.Drawable.ic_lock);

                    if (item.PrivacyModelClass.GroupClass.Members != 0)
                        holder.TxtMembers.Text = Methods.FunString.FormatPriceValue(item.PrivacyModelClass.GroupClass.Members) + " " + ActivityContext.GetString(Resource.String.Lbl_Members);
                    else if (item.PrivacyModelClass.GroupClass.MembersCount != null && item.PrivacyModelClass.GroupClass.MembersCount.Value != 0)
                        holder.TxtMembers.Text = Methods.FunString.FormatPriceValue(item.PrivacyModelClass.GroupClass.MembersCount.Value) + " " + ActivityContext.GetString(Resource.String.Lbl_Members);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InfoPageBoxBind(AdapterHolders.InfoPageBoxViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                if (item.PageInfoModelClass?.PageClass != null)
                {
                    //Extra  
                    holder.LikeCountText.Text = item.PageInfoModelClass?.PageClass.LikesCount;

                    if (item.PageInfoModelClass.PageClass.IsPageOnwer != null && item.PageInfoModelClass.PageClass.IsPageOnwer.Value)
                    {
                        holder.RatingLiner.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        holder.RatingLiner.Visibility = ViewStates.Visible;
                    }
                    holder.RatingBarView.Rating = Float.ParseFloat(item.PageInfoModelClass.PageClass.Rating);

                    holder.CategoryText.Text = new CategoriesController().Get_Translate_Categories_Communities(item.PageInfoModelClass.PageClass.PageCategory, item.PageInfoModelClass.PageClass.Category, "Page");

                    //if (Methods.FunString.StringNullRemover(item.PageInfoModelClass.PageClass.About) != "Empty")
                    //{
                    //    var about = Methods.FunString.DecodeString(item.PageInfoModelClass.PageClass.About);
                    //    NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.AboutDesc, new String(about));
                    //}
                    //else
                    //{
                    //    holder.AboutDesc.Text = ActivityContext.GetText(Resource.String.Lbl_NoAnyDescription);
                    //}
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StoryBind(AdapterHolders.StoryViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.StoryAdapter.StoryList = item.StoryList.Count switch
                {
                    > 0 => new ObservableCollection<StoryDataObject>(item.StoryList),
                    _ => holder.StoryAdapter.StoryList
                };

                var dataOwner = holder.StoryAdapter.StoryList.FirstOrDefault(a => a.Type == "Your");
                switch (dataOwner)
                {
                    case null:
                        holder.StoryAdapter.StoryList.Insert(0, new StoryDataObject
                        {
                            Avatar = UserDetails.Avatar,
                            Type = "Your",
                            Username = ActivityContext.GetText(Resource.String.Lbl_YourStory),
                            Stories = new List<UserDataStory>
                            {
                                new UserDataStory
                                {
                                    Thumbnail = UserDetails.Avatar,
                                }
                            }
                        });
                        break;
                }

                holder.StoryAdapter.NotifyDataSetChanged();

                holder.AboutMore.Visibility = holder.StoryAdapter?.StoryList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FollowersBoxBind(AdapterHolders.FollowersViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.FollowersAdapter?.MUserFriendsList?.Count)
                {
                    case 0:
                        holder.FollowersAdapter.MUserFriendsList = new ObservableCollection<UserDataObject>(item.FollowersModel.FollowingList);
                        holder.FollowersAdapter.NotifyDataSetChanged();
                        break;
                }

                holder.TitleText.Text = item.FollowersModel.TitleHead;

                if (!string.IsNullOrEmpty(item.FollowersModel.Description))
                {
                    holder.DescriptionText.Visibility = ViewStates.Visible;
                    holder.DescriptionText.Text = item.FollowersModel.Description;
                }
                else
                {
                    holder.DescriptionText.Visibility = ViewStates.Gone;
                }

                holder.MoreText.Text = item.FollowersModel.More;
                holder.MoreLayout.Visibility = holder.FollowersAdapter?.MUserFriendsList?.Count > 5 ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void GroupsBoxBind(AdapterHolders.GroupsViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.GroupsAdapter?.GroupList?.Count)
                {
                    case 0:
                        holder.GroupsAdapter.GroupList = new ObservableCollection<GroupDataObject>(item.GroupsModel.GroupsList);
                        holder.GroupsAdapter.NotifyDataSetChanged();
                        break;
                }

                holder.AboutHead.Text = string.IsNullOrEmpty(item.GroupsModel?.TitleHead) switch
                {
                    false => item.GroupsModel?.TitleHead,
                    _ => holder.AboutHead.Text
                };
                holder.AboutHead.Text = holder.AboutHead.Text.ToUpper();
                holder.AboutMore.Text = item.GroupsModel?.More;

                if (holder.GroupsAdapter != null)
                {
                    holder.AboutMore.Visibility = holder.GroupsAdapter?.GroupList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SuggestedPagesBoxBind(AdapterHolders.SuggestedPagesViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.PagesAdapter?.PageList?.Count)
                {
                    case 0:
                        holder.PagesAdapter.PageList = new ObservableCollection<PageDataObject>(ListUtils.SuggestedPageList.Take(12));
                        holder.PagesAdapter.NotifyDataSetChanged();
                        holder.AboutMore.Text = ListUtils.SuggestedPageList.Count.ToString();

                        holder.AboutMore.Visibility = holder.PagesAdapter?.PageList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SuggestedGroupsBoxBind(AdapterHolders.SuggestedGroupsViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.GroupsAdapter?.GroupList?.Count)
                {
                    case 0:
                        holder.GroupsAdapter.GroupList = new ObservableCollection<GroupDataObject>(ListUtils.SuggestedGroupList.Take(12));
                        holder.GroupsAdapter.NotifyDataSetChanged();
                        holder.AboutMore.Text = ListUtils.SuggestedGroupList.Count.ToString();

                        holder.AboutMore.Visibility = holder.GroupsAdapter?.GroupList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SuggestedUsersBoxBind(AdapterHolders.SuggestedUsersViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.UsersAdapter?.UserList?.Count)
                {
                    case 0:
                        holder.UsersAdapter.UserList = new ObservableCollection<UserDataObject>(ListUtils.SuggestedUserList.Take(12));
                        holder.UsersAdapter.NotifyDataSetChanged();
                        holder.AboutMore.Text = ListUtils.SuggestedUserList.Count.ToString();

                        holder.AboutMore.Visibility = holder.UsersAdapter?.UserList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ImagesBoxBind(AdapterHolders.ImagesViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.AboutHead.Text = string.IsNullOrEmpty(item.ImagesModel.TitleHead) switch
                {
                    false => item.ImagesModel.TitleHead,
                    _ => holder.AboutHead.Text
                };

                holder.AboutHead.Text = holder.AboutHead.Text.ToUpper();
                holder.AboutMore.Text = item.ImagesModel.More;

                switch (item.ImagesModel?.ImagesList)
                {
                    case null:
                        holder.MainView.Visibility = ViewStates.Gone;
                        return;
                }

                if (holder.MainView.Visibility != ViewStates.Visible)
                    holder.MainView.Visibility = ViewStates.Visible;

                switch (holder.ImagesAdapter?.UserPhotosList?.Count)
                {
                    case 0:
                        holder.ImagesAdapter.UserPhotosList = new ObservableCollection<PostDataObject>(item.ImagesModel.ImagesList);
                        holder.ImagesAdapter.NotifyDataSetChanged();
                        break;
                }

                holder.AboutMore.Visibility = holder.ImagesAdapter?.UserPhotosList?.Count > 3 ? ViewStates.Visible : ViewStates.Invisible;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PagesBoxBind(AdapterHolders.PagesViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                switch (holder.PagesAdapter?.PageList?.Count)
                {
                    case 0:
                        holder.PagesAdapter.PageList = new ObservableCollection<PageDataObject>(item.PagesModel.PagesList);
                        holder.PagesAdapter.NotifyDataSetChanged();
                        break;
                }

                holder.AboutHead.Text = string.IsNullOrEmpty(item.PagesModel?.TitleHead) switch
                {
                    false => item.PagesModel?.TitleHead,
                    _ => holder.AboutHead.Text
                };
                holder.AboutHead.Text = holder.AboutHead.Text.ToUpper();
                holder.AboutMore.Text = item.PagesModel?.More;

                if (holder.PagesAdapter != null)
                {
                    holder.AboutMore.Visibility = holder.PagesAdapter?.PageList?.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        /*public void PagesBoxBind(AdapterHolders.PagesViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                holder.AboutHead.Text = string.IsNullOrEmpty(item.PagesModel?.TitleHead) switch
                {
                    false => item.PagesModel?.TitleHead,
                    _ => holder.AboutHead.Text
                };

                holder.AboutMore.Text = string.IsNullOrEmpty(item.PagesModel?.More) switch
                {
                    false => item.PagesModel?.More,
                    _ => holder.AboutMore.Text
                };

                var count = item.PagesModel?.PagesList.Count;
                Console.WriteLine(count);

                try
                {
                    switch (item.PagesModel?.PagesList.Count)
                    {
                        case > 0 when !string.IsNullOrEmpty(item.PagesModel?.PagesList[0]?.Avatar):
                            GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[0]?.Avatar, holder.PageImage1, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            break;
                    }

                    switch (item.PagesModel?.PagesList.Count)
                    {
                        case > 1 when !string.IsNullOrEmpty(item.PagesModel?.PagesList[1]?.Avatar):
                            GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[1]?.Avatar, holder.PageImage2, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            break;
                    }

                    switch (item.PagesModel?.PagesList.Count)
                    {
                        case > 2 when !string.IsNullOrEmpty(item.PagesModel?.PagesList[2]?.Avatar):
                            GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[2]?.Avatar, holder.PageImage1, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }*/

        public void AdsPostBind(AdapterHolders.AdsPostViewHolder holder, AdapterModelsClass item)
        {
            try
            {
                LoadImage(item.PostData.UserData.Avatar, holder.UserAvatar);

                if (string.IsNullOrEmpty(item.PostData.AdMedia))
                    holder.Image.Visibility = ViewStates.Gone;
                else
                    LoadImage(item.PostData.AdMedia, holder.Image);

                holder.Username.Text = item.PostData.Name;
                holder.TimeText.Text = item.PostData.Posted;
                holder.TextLocation.Text = item.PostData.Location;

                if (string.IsNullOrEmpty(item.PostData.Orginaltext))
                {
                    if (holder.Description.Visibility != ViewStates.Gone)
                        holder.Description.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (holder.Description.Visibility != ViewStates.Visible)
                        holder.Description.Visibility = ViewStates.Visible;

                    if (!holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadMore)) && !holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                    {
                        switch (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                        {
                            case true:
                                holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, item.PostData.RegexFilterList);
                                break;
                            default:
                                holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());
                                break;
                        }

                        NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.Description, new String(item.PostData.Orginaltext));
                    }
                    //else if (holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                    //{
                    //    NativePostAdapter.ReadMoreOption.AddReadLess(holder.Description, new String(item.PostData.Orginaltext));
                    //}
                    else
                    {
                        switch (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                        {
                            case true:
                                holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, item.PostData.RegexFilterList);
                                break;
                            default:
                                holder.Description.SetAutoLinkOnClickListener(NativePostAdapter, new Dictionary<string, string>());
                                break;
                        }

                        NativePostAdapter.ReadMoreOption.AddReadMoreTo(holder.Description, new String(item.PostData.Orginaltext));
                    }
                }

                TextSanitizer headlineSanitizer = new TextSanitizer(holder.Headline, ActivityContext);
                headlineSanitizer.Load(item.PostData.Headline);

                holder.LinkUrl.Text = item.PostData.Url;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region GlideCustom


        public static List<int> HightImagePreloader = new List<int>();
        public class GlideCustomRequestListener : Object, IRequestListener
        {
            string typeofload = "Default";
            public GlideCustomRequestListener(string type)
            {
                typeofload = type;
            }


            public bool OnLoadFailed(GlideException p0, Object p1, ITarget p2, bool p3)
            {

                foreach (var d in p0.Causes)
                {
                    Console.WriteLine("WoLog: Glide / GlideException Cause = " + d.Cause + " Object= " + p1);
                }

                p2.GetSize(new GlideIsize());

                return false;
            }

            public bool OnResourceReady(Object p0, Object p1, ITarget p2, DataSource p3, bool p4)
            {
                try
                {
                    Bitmap bitmap = null;

                    if (p0 is Bitmap myObject)
                        bitmap = myObject;
                    else if (p0 is BitmapDrawable drawable)
                        bitmap = drawable.Bitmap;

                    if (bitmap == null)
                        return false;

                    //Android.Graphics.Bitmap bitmap = ((BitmapDrawable)p0).Bitmap;
                    //Android.Graphics.Bitmap bitmap = ((Android.Graphics.Bitmap)p0);

                    var BytehasKB = Math.Round((double)bitmap.ByteCount / 1024, 0);

                    Console.WriteLine("WoLog: Glide TYPE: " + typeofload + " / OnResourceReady URL: " + p1 + " Width = " + bitmap.GetBitmapInfo().Width + " Height= " + bitmap.GetBitmapInfo().Height + " AllocationByteCount = " + bitmap.AllocationByteCount + " ByteCount = " + bitmap.ByteCount + " to KB = " + BytehasKB);
                    p2.GetSize(new GlideIsize());

                    var Aspect = bitmap.GetBitmapInfo().Width / bitmap.GetBitmapInfo().Height;
                    if (bitmap.GetBitmapInfo().Height > bitmap.GetBitmapInfo().Width)
                    {

                        // Console.WriteLine("WoLog: Glide ScaleType: Height: " + bitmap.GetBitmapInfo().Height + " x Width:" + bitmap.GetBitmapInfo().Width + " Aspect:" + Aspect);
                    }

                    //var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(new ColorDrawable(Color.ParseColor("#EFEFEF"))).Error(Resource.Drawable.ImagePlacholder).Format(Bumptech.Glide.Load.DecodeFormat.PreferRgb565).Apply(RequestOptions.SignatureOf(new ObjectKey(DateTime.Now.Millisecond)));
                    //var FullGlideRequestBuilder = Glide.With(holder.ItemView).AsBitmap().Thumbnail(Glide.With(holder.ItemView).AsDrawable().SetSizeMultiplier(0.8f).Downsample(DownsampleStrategy.AtMost)).SetSizeMultiplier(0.5f).Apply(glideRequestOptions).Timeout(3000);
                    //FullGlideRequestBuilder.DontTransform_T();
                    //FullGlideRequestBuilder.Downsample(DownsampleStrategy.CenterInside).CenterInside();
                    //FullGlideRequestBuilder.AddListener(new GlideCustomRequestListener()).Load(imageUrl).CenterInside().Into(holder.Image);

                    return false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;
                }
            }
        }

        public class GlidePreLoaderRequestListener : Object, IRequestListener
        {
            string typeofload = "Default";
            public GlidePreLoaderRequestListener(string type)
            {
                typeofload = type;
            }


            public bool OnLoadFailed(GlideException p0, Object p1, ITarget p2, bool p3)
            {

                foreach (var d in p0.Causes)
                {
                    Console.WriteLine("WoLog: Glide / GlideException Cause = " + d.Cause + " Object= " + p1);
                }

                p2.GetSize(new GlideIsize());



                return false;
            }


            public bool OnResourceReady(Object p0, Object p1, ITarget p2, DataSource p3, bool p4)
            {
                try
                { //Glide gif drawble
                    Bitmap bitmap;
                    var myObject = p0 as Bitmap;

                    if (myObject != null)
                        bitmap = ((Bitmap)p0);
                    else
                        bitmap = ((BitmapDrawable)p0).Bitmap;

                    var BytehasKB = Math.Round((double)bitmap.ByteCount / 1024, 0);

                    if (typeofload == "Preload Normal")
                        HightImagePreloader.Add((int)bitmap.GetBitmapInfo().Height);

                    Console.WriteLine("WoLog: Glide Preload TYPE: " + typeofload + " / OnResourceReady URL: " + p1 + " Width = " + bitmap.GetBitmapInfo().Width + " Height= " + bitmap.GetBitmapInfo().Height + " AllocationByteCount = " + bitmap.AllocationByteCount + " ByteCount = " + bitmap.ByteCount + " to KB = " + BytehasKB);

                    var aspect = bitmap.GetBitmapInfo().Width / bitmap.GetBitmapInfo().Height;
                    if (bitmap.GetBitmapInfo().Height > bitmap.GetBitmapInfo().Width)
                    {
                        Console.WriteLine("WoLog: Glide ScaleType: Height: " + bitmap.GetBitmapInfo().Height + " x Width:" + bitmap.GetBitmapInfo().Width + " Aspect:" + aspect);
                    }

                    p2.GetSize(new GlideIsize());

                    return false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return false;
                }

            }
        }

        public class GlideIsize : Object, ISizeReadyCallback
        {
            public void OnSizeReady(int p0, int p1)
            {
                // Console.WriteLine("WoLog: Glide / OnSizeReady Width = " + p0 + " Height= " + p1);
            }
        }


        #endregion

    }
}