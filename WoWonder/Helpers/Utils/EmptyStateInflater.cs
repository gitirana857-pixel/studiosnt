using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Com.Airbnb.Lottie;
using System;
using WoWonder.Helpers.Fonts;

namespace WoWonder.Helpers.Utils
{
    public class EmptyStateInflater
    {
        public AppCompatButton EmptyStateButton;
        private TextView EmptyStateIcon;
        private TextView DescriptionText;
        private TextView TitleText;
        private LottieAnimationView AnimationView;
        private ImageView EmptyImage;

        public enum Type
        {
            NoConnection,
            NoSearchResult,
            SomThingWentWrong,
            NoPost,
            NoComments,
            NoNotifications,
            NoUsers,
            NoUsersReaction,
            NoFollow,
            NoAlbum,
            NoArticle,
            NoMovies,
            NoNearBy,
            NoEvent,
            NoProduct,
            NoGroup,
            NoPage,
            NoPhoto,
            NoFunding,
            NoJob,
            NoJobApply,
            NoCommonThings,
            NoReviews,
            NoVideo,
            NoGames,
            NoSessions,
            Gify,
            NoActivities,
            NoMemories,
            NoOffers,
            NoShop,
            NoBusiness,
            NoBlockedUsers,
            NoCarts,
            NoAddress,

            //Chat
            NoCall,
            NoGroupChat,
            NoPageChat,
            NoMessages,
            NoFiles,
            NoGroupRequest,
            NoStartedMessages,
            NoPinnedMessages,
            NoArchive,
            NoBroadcast,
        }

        public void InflateLayout(View inflated, Type type)
        {
            try
            {
                EmptyStateIcon = (TextView)inflated.FindViewById(Resource.Id.emtyicon);
                TitleText = (TextView)inflated.FindViewById(Resource.Id.headText);
                DescriptionText = (TextView)inflated.FindViewById(Resource.Id.seconderyText);
                EmptyStateButton = (AppCompatButton)inflated.FindViewById(Resource.Id.button);
                AnimationView = inflated.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
                EmptyImage = inflated.FindViewById<ImageView>(Resource.Id.iv_empty);

                EmptyImage.Visibility = ViewStates.Gone;
                AnimationView.Visibility = ViewStates.Gone;
                EmptyStateIcon.Visibility = ViewStates.Gone;
                EmptyStateButton.Visibility = ViewStates.Gone;

                switch (type)
                {
                    case Type.NoConnection:
                        AnimationView.SetAnimation("NoInterntconnection.json");
                        AnimationView.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoConnection_Button);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoSearchResult:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Search);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_NoSearchResult_Button);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_result);
                        EmptyImage.Visibility = ViewStates.Visible;
                        break;
                    case Type.SomThingWentWrong:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Close);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_DescriptionText);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_SomThingWentWrong_Button);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoComments:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Chatbubbles);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoComments);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoPost:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.ImagePolaroid);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoPost_TitleText);
                        DescriptionText.Text = " ";
                        break;
                    case Type.NoNotifications:
                        AnimationView.SetAnimation("EmptyStateAnim4.json");
                        AnimationView.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_notifications);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoNotifcationsDescriptions);
                        break;
                    case Type.NoUsers:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_DescriptionText);
                        break;
                    case Type.NoUsersReaction:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = " ";
                        break;
                    case Type.NoFollow:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoFollow_DescriptionText);
                        break;
                    case Type.NoAlbum:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Images);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Albums);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Albums);
                        break;
                    case Type.NoArticle:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.FileAlt);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Article);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Article);
                        break;
                    case Type.NoMovies:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Video);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_movies);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Movies);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Movies);
                        break;
                    case Type.NoNearBy:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_NearBy);
                        break;
                    case Type.NoEvent:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_events);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Events);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Events);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Btn_Create_Events);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoProduct:
                        //AnimationView.SetAnimation("EmptyStateAnim1.json");
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_products);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Market);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Market);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Btn_AddProduct);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoGroup:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_JoinedGroup);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_JoinedGroup);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_Search);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoPage:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.CalendarAlt);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_LikedPages);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_LikedPages);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_Search);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoPhoto:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Images);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Albums);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Albums);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoFunding:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.HandHoldingUsd);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_funding);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NotHaveAnyFundingRequest);
                        DescriptionText.Text = " ";
                        break;
                    case Type.NoJob:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_jobs);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NotHaveAnyJobs);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoJobsDescriptions);
                        break;
                    case Type.NoJobApply:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Briefcase);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = " ";
                        break;
                    case Type.NoCommonThings:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoCommentThingsDescriptions);
                        break;
                    case Type.NoReviews:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Star);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoReviews);
                        DescriptionText.Text = " ";
                        break;
                    case Type.NoVideo:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.VideoSlash);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Video);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Video);
                        EmptyStateButton.Text = Application.Context.GetText(Resource.String.Lbl_Add);
                        EmptyStateButton.Visibility = ViewStates.Visible;
                        break;
                    case Type.NoGames:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Gamepad);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_games);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Games);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Games_Desc);
                        break;
                    case Type.NoSessions:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Fingerprint);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Sessions);
                        DescriptionText.Text = "";
                        break;
                    case Type.Gify:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Gift);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Gif);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoActivities:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.ChartLine);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Activities);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoMemories:
                        AnimationView.SetAnimation("EmptyStateAnim6.json");
                        AnimationView.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Memories);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoMemoriesDescriptions);
                        break;
                    case Type.NoOffers:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EmptyStateIcon, FontAwesomeIcon.Box);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_offers);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Offers);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoShop:
                        AnimationView.SetAnimation("EmptyStateAnim1.json");
                        AnimationView.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Shop);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoBusiness:
                        AnimationView.SetAnimation("EmptyStateAnim3.json");
                        AnimationView.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Business);
                        DescriptionText.Text = "";
                        break;
                    case Type.NoBlockedUsers:
                        EmptyImage.SetImageResource(Resource.Drawable.ic_no_user);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_NoUsers_TitleText);
                        DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_NoBlockedUsersDesc);
                        break;
                    case Type.NoCarts:
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Cart);
                        EmptyStateIcon.SetTextSize(ComplexUnitType.Dip, 45f);
                        EmptyStateIcon.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Carts);
                        DescriptionText.Text = " ";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                    case Type.NoAddress:
                        EmptyImage.SetImageResource(Resource.Drawable.icon_address_vector);
                        EmptyImage.Visibility = ViewStates.Visible;
                        TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Empty_Address);
                        DescriptionText.Text = " ";
                        EmptyStateButton.Visibility = ViewStates.Gone;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}