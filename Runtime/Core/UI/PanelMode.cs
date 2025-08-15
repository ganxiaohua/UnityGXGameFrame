namespace GameFrame.Runtime
{
    public enum PanelMode
    {
        /// <summary>
        /// Base panel
        /// </summary>
        Normal,

        /// <summary>
        /// Monopolize display panel
        /// </summary>
        Mono,

        /// <summary>
        /// Subpanel embedded in other panels
        /// </summary>
        Embed,

        /// <summary>
        /// Pop-up panel docked to other panels
        /// </summary>
        Pop,
    }
}