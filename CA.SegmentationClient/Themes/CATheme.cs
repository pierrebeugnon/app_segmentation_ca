using MudBlazor;

namespace CA.SegmentationClient.Themes
{
    public static class CATheme
    {
        public static MudTheme Theme => new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary           = "#005B2B",
                PrimaryContrastText = "#FFFFFF",
                Secondary         = "#1A4E6B",
                SecondaryContrastText = "#FFFFFF",
                Tertiary          = "#78BE20",
                TertiaryContrastText = "#FFFFFF",
                Background        = "#F0F2F5",
                BackgroundGray    = "#E8EDF2",
                Surface           = "#FFFFFF",
                AppbarBackground  = "#005B2B",
                AppbarText        = "#FFFFFF",
                DrawerBackground  = "#1A4E6B",
                DrawerText        = "#FFFFFF",
                DrawerIcon        = "rgba(255,255,255,0.7)",
                Success           = "#27AE60",
                Warning           = "#E87722",
                Error             = "#C8102E",
                Info              = "#1A4E6B",
                TextPrimary       = "#2C3E50",
                TextSecondary     = "#6C7A89",
                TextDisabled      = "rgba(44,62,80,.38)",
                ActionDefault     = "#2C3E50",
                ActionDisabled    = "rgba(44,62,80,.26)",
                ActionDisabledBackground = "rgba(44,62,80,.12)",
                Divider           = "#D8DCE0",
                DividerLight      = "rgba(216,220,224,.4)",
                TableLines        = "#D8DCE0",
                TableHover        = "#F7F9FB",
                TableStriped      = "#F7F9FB",
                OverlayDark       = "rgba(18,55,76,.6)",
                LinesDefault      = "#D8DCE0",
                LinesInputs       = "#D8DCE0",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "6px",
                DrawerWidthLeft     = "250px",
            }
        };
    }
}
