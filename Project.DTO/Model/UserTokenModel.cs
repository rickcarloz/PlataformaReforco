namespace Project.DTO.Model
{

    public class UserTokenModel
    {

        public bool IsAuthenticated { get; set; }
        public UserModel User { get; set; }
        public string? Access_token { get; set; }
        public string? Message { get; set; }
    }


    public class DataModel
    {
        public string? displayName { get; set; }
        public string? photoURL { get; set; }
        public string? email { get; set; }
        public SettingsModel settings { get; set; }
        public List<string> shortcuts { get; set; }
        public List<Modules> modules { get; set; }

    }



    public class UserModel
    {
        public string? uuid { get; set; }
        public string? userName { get; set; }
        public string? from { get; set; }
        public string? role { get; set; }
        public bool Admin { get; set; }
        public string loginRedirectUrl { get; set; }
        public DataModel data { get; set; }
    }



    public class Modules
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public string? title { get; set; }
        public string? subtitle { get; set; }
        public string? icon { get; set; }
        public string? url { get; set; }
        public string? auth { get; set; }
        public List<Modules> children { get; set; }
    }




    public class SettingsModel
    {
        public bool customScrollbars { get; set; }
        public string? direction { get; set; }
        public Theme theme { get; set; }
        public Layout layout { get; set; }
        public List<string> defaultAuth { get; set; }
        public string? loginRedirectUrl { get; set; }
    }


    public class Background
    {
        public string? paper { get; set; }
        public string? @default { get; set; }
    }

    public class Common
    {
        public string? black { get; set; }
        public string? white { get; set; }
    }

    public class Config
    {
        public string? mode { get; set; }
        public int containerWidth { get; set; }
        public Navbar navbar { get; set; }
        public Toolbar toolbar { get; set; }
        public Footer footer { get; set; }
        public LeftSidePanel leftSidePanel { get; set; }
        public RightSidePanel rightSidePanel { get; set; }
        public SettingsPanel settingsPanel { get; set; }
    }

    public class Error
    {
        public string? light { get; set; }
        public string? main { get; set; }
        public string? dark { get; set; }
    }

    public class Footer
    {
        public Palette palette { get; set; }
        public bool display { get; set; }
        public string? style { get; set; }
    }

    public class Layout
    {
        public string? style { get; set; }
        public Config config { get; set; }
    }

    public class LeftSidePanel
    {
        public bool display { get; set; }
    }

    public class Main
    {
        public Palette palette { get; set; }
        public Status status { get; set; }
    }

    public class Navbar
    {
        public Palette palette { get; set; }
        public bool display { get; set; }
        public string? style { get; set; }
        public bool folded { get; set; }
        public string? position { get; set; }
    }

    public class Palette
    {
        public string? mode { get; set; }
        public Text text { get; set; }
        public Common common { get; set; }
        public Primary primary { get; set; }
        public Secondary secondary { get; set; }
        public Background background { get; set; }
        public Error error { get; set; }
        public string? divider { get; set; }
        public Status status { get; set; }
    }

    public class Primary
    {
        public string? light { get; set; }
        public string? main { get; set; }
        public string? dark { get; set; }
        public string? contrastDefaultColor { get; set; }
        public string? contrastText { get; set; }
    }

    public class RightSidePanel
    {
        public bool display { get; set; }
    }


    public class Secondary
    {
        public string? light { get; set; }
        public string? main { get; set; }
        public string? dark { get; set; }
        public string? contrastText { get; set; }
    }

    public class SettingsPanel
    {
        public bool display { get; set; }
    }

    public class Status
    {
        public string? danger { get; set; }
    }

    public class Text
    {
        public string? primary { get; set; }
        public string? secondary { get; set; }
        public string? disabled { get; set; }
    }

    public class Theme
    {
        public Main main { get; set; }
        public Navbar navbar { get; set; }
        public Toolbar toolbar { get; set; }
        public Footer footer { get; set; }
    }

    public class Toolbar
    {
        public Palette palette { get; set; }
        public Status status { get; set; }
        public bool display { get; set; }
        public string? style { get; set; }
    }
}
