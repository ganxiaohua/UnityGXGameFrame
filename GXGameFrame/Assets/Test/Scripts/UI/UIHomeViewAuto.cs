using FairyGUI;
using GameFrame;

public partial class UIHomeView : UIViewBase
{
    private GButton m_BtnDevelop;

    private GButton BtnDevelop
    {
        get {
            if (m_BtnDevelop == null)
            {
                m_BtnDevelop = contentPane.GetChildByPath("HomeMainPanel.BtnAdventure").asButton;
            }
            return m_BtnDevelop;
        }
    }
}