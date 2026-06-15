namespace WoWonder.Helpers.Model
{
    public enum PostButtonSystem
    {
        Wonder,
        DisLike,
        Like,
        Reaction,
    }

    public enum MoreTheme
    {
        Card,
        Grid,
    }

    public enum ToastTheme
    {
        Default,
        Custom,
    }

    public enum StyleRowMore
    {
        Row = 0,
        Card = 1,
        Grid = 2,
    }

    public enum VideoPostTypeSystem
    {
        EmbedVideo,
        Link,
        None
    }


    public enum CoverImageStyle
    {
        CenterCrop,
        FitCenter,
        Default
    }

    public enum ImagePostStyle
    {
        FullWidth,
        Default
    }

    public enum SystemGetLastChat
    {
        MultiTab,
        Default
    }

    public enum SystemCall
    {
        Agora,
        Twilio
    }

    public enum EnableCall
    {
        AudioAndVideo,
        OnlyAudio,
        OnlyVideo,
        Disable
    }

    public enum ChatTheme
    {
        Default,
        Tokyo,
    }

    public enum SystemLive
    {
        Agora,
        Millicast
    }

    public enum ColorMessageTheme
    {
        Default,
        Gradient,
    }

    public enum ShowAds
    {
        AllUsers,
        UnProfessional,
    }

    public enum AddPostSystem
    {
        AllUsers,
        OnlyAdmin,
    }

    public enum ReelsPosition
    {
        ToolBar,
        Tab,
        None,
    }

    public enum TabTheme
    {
        Dark,
        Light,
    }

    public enum AppMode
    {
        Default,
        //Instagram,
        LinkedIn,
    }

    public enum GalleryIntentSystem
    {
        Default,
        Pix,
    }

}