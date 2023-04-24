using FairyGUI;
using GameFrame;
namespace GXGame
{
    public partial class UICardWindowView : UIViewBase
    {
        
        private GLoader m_Bg;
        private GLoader Bg
        {
            get {
                if (m_Bg == null)
                {
                    m_Bg = contentPane.GetChildByPath("Bg") as GLoader;
                }
                return m_Bg;
            }
        }
        private GComponent m_EffectRoot01;
        private GComponent EffectRoot01
        {
            get {
                if (m_EffectRoot01 == null)
                {
                    m_EffectRoot01 = contentPane.GetChildByPath("EffectRoot01") as GComponent;
                }
                return m_EffectRoot01;
            }
        }
        private GImage m_n343;
        private GImage n343
        {
            get {
                if (m_n343 == null)
                {
                    m_n343 = contentPane.GetChildByPath("n343") as GImage;
                }
                return m_n343;
            }
        }
        private GComponent m_ModelLoader;
        private GComponent ModelLoader
        {
            get {
                if (m_ModelLoader == null)
                {
                    m_ModelLoader = contentPane.GetChildByPath("ModelLoader") as GComponent;
                }
                return m_ModelLoader;
            }
        }
        private GImage m_Mask;
        private GImage Mask
        {
            get {
                if (m_Mask == null)
                {
                    m_Mask = contentPane.GetChildByPath("Mask") as GImage;
                }
                return m_Mask;
            }
        }
        private GImage m_n337;
        private GImage n337
        {
            get {
                if (m_n337 == null)
                {
                    m_n337 = contentPane.GetChildByPath("n337") as GImage;
                }
                return m_n337;
            }
        }
        private GImage m_n75;
        private GImage n75
        {
            get {
                if (m_n75 == null)
                {
                    m_n75 = contentPane.GetChildByPath("n75") as GImage;
                }
                return m_n75;
            }
        }
        private GList m_n9;
        private GList n9
        {
            get {
                if (m_n9 == null)
                {
                    m_n9 = contentPane.GetChildByPath("n9") as GList;
                }
                return m_n9;
            }
        }
        private GComponent m_FilterBtn;
        private GComponent FilterBtn
        {
            get {
                if (m_FilterBtn == null)
                {
                    m_FilterBtn = contentPane.GetChildByPath("FilterBtn") as GComponent;
                }
                return m_FilterBtn;
            }
        }
        private GButton m_ChangeCardBtn;
        private GButton ChangeCardBtn
        {
            get {
                if (m_ChangeCardBtn == null)
                {
                    m_ChangeCardBtn = contentPane.GetChildByPath("ChangeCardBtn") as GButton;
                }
                return m_ChangeCardBtn;
            }
        }
        private GComponent m_ChangeCardBtn_ModelLoader;
        private GComponent ChangeCardBtn_ModelLoader
        {
            get {
                if (m_ChangeCardBtn_ModelLoader == null)
                {
                    m_ChangeCardBtn_ModelLoader = contentPane.GetChildByPath("ChangeCardBtn.ModelLoader") as GComponent;
                }
                return m_ChangeCardBtn_ModelLoader;
            }
        }
        private GImage m_ChangeCardBtn_n4;
        private GImage ChangeCardBtn_n4
        {
            get {
                if (m_ChangeCardBtn_n4 == null)
                {
                    m_ChangeCardBtn_n4 = contentPane.GetChildByPath("ChangeCardBtn.n4") as GImage;
                }
                return m_ChangeCardBtn_n4;
            }
        }
        private GImage m_PowerItemBg;
        private GImage PowerItemBg
        {
            get {
                if (m_PowerItemBg == null)
                {
                    m_PowerItemBg = contentPane.GetChildByPath("PowerItemBg") as GImage;
                }
                return m_PowerItemBg;
            }
        }
        private GTextField m_NumberText;
        private GTextField NumberText
        {
            get {
                if (m_NumberText == null)
                {
                    m_NumberText = contentPane.GetChildByPath("NumberText") as GTextField;
                }
                return m_NumberText;
            }
        }
        private GTextField m_Zhanli;
        private GTextField Zhanli
        {
            get {
                if (m_Zhanli == null)
                {
                    m_Zhanli = contentPane.GetChildByPath("Zhanli") as GTextField;
                }
                return m_Zhanli;
            }
        }
        private GLoader m_Icon;
        private GLoader Icon
        {
            get {
                if (m_Icon == null)
                {
                    m_Icon = contentPane.GetChildByPath("Icon") as GLoader;
                }
                return m_Icon;
            }
        }
        private GImage m_ArrowsUp;
        private GImage ArrowsUp
        {
            get {
                if (m_ArrowsUp == null)
                {
                    m_ArrowsUp = contentPane.GetChildByPath("ArrowsUp") as GImage;
                }
                return m_ArrowsUp;
            }
        }
        private GImage m_ArrowsDown;
        private GImage ArrowsDown
        {
            get {
                if (m_ArrowsDown == null)
                {
                    m_ArrowsDown = contentPane.GetChildByPath("ArrowsDown") as GImage;
                }
                return m_ArrowsDown;
            }
        }
        private GComponent m_DetailsPanel;
        private GComponent DetailsPanel
        {
            get {
                if (m_DetailsPanel == null)
                {
                    m_DetailsPanel = contentPane.GetChildByPath("DetailsPanel") as GComponent;
                }
                return m_DetailsPanel;
            }
        }
        private GImage m_DetailsPanel_Line;
        private GImage DetailsPanel_Line
        {
            get {
                if (m_DetailsPanel_Line == null)
                {
                    m_DetailsPanel_Line = contentPane.GetChildByPath("DetailsPanel.Line") as GImage;
                }
                return m_DetailsPanel_Line;
            }
        }
        private GImage m_DetailsPanel_n46;
        private GImage DetailsPanel_n46
        {
            get {
                if (m_DetailsPanel_n46 == null)
                {
                    m_DetailsPanel_n46 = contentPane.GetChildByPath("DetailsPanel.n46") as GImage;
                }
                return m_DetailsPanel_n46;
            }
        }
        private GImage m_DetailsPanel_n47;
        private GImage DetailsPanel_n47
        {
            get {
                if (m_DetailsPanel_n47 == null)
                {
                    m_DetailsPanel_n47 = contentPane.GetChildByPath("DetailsPanel.n47") as GImage;
                }
                return m_DetailsPanel_n47;
            }
        }
        private GTextField m_DetailsPanel_TitleText;
        private GTextField DetailsPanel_TitleText
        {
            get {
                if (m_DetailsPanel_TitleText == null)
                {
                    m_DetailsPanel_TitleText = contentPane.GetChildByPath("DetailsPanel.TitleText") as GTextField;
                }
                return m_DetailsPanel_TitleText;
            }
        }
        private GImage m_DetailsPanel_Line1;
        private GImage DetailsPanel_Line1
        {
            get {
                if (m_DetailsPanel_Line1 == null)
                {
                    m_DetailsPanel_Line1 = contentPane.GetChildByPath("DetailsPanel.Line") as GImage;
                }
                return m_DetailsPanel_Line1;
            }
        }
        private GImage m_DetailsPanel_n71;
        private GImage DetailsPanel_n71
        {
            get {
                if (m_DetailsPanel_n71 == null)
                {
                    m_DetailsPanel_n71 = contentPane.GetChildByPath("DetailsPanel.n71") as GImage;
                }
                return m_DetailsPanel_n71;
            }
        }
        private GImage m_DetailsPanel_n72;
        private GImage DetailsPanel_n72
        {
            get {
                if (m_DetailsPanel_n72 == null)
                {
                    m_DetailsPanel_n72 = contentPane.GetChildByPath("DetailsPanel.n72") as GImage;
                }
                return m_DetailsPanel_n72;
            }
        }
        private GTextField m_DetailsPanel_TitleText2;
        private GTextField DetailsPanel_TitleText2
        {
            get {
                if (m_DetailsPanel_TitleText2 == null)
                {
                    m_DetailsPanel_TitleText2 = contentPane.GetChildByPath("DetailsPanel.TitleText") as GTextField;
                }
                return m_DetailsPanel_TitleText2;
            }
        }
        private GList m_DetailsPanel_PropertyGroup;
        private GList DetailsPanel_PropertyGroup
        {
            get {
                if (m_DetailsPanel_PropertyGroup == null)
                {
                    m_DetailsPanel_PropertyGroup = contentPane.GetChildByPath("DetailsPanel.PropertyGroup") as GList;
                }
                return m_DetailsPanel_PropertyGroup;
            }
        }
        private GComponent m_DetailsPanel_Skill01;
        private GComponent DetailsPanel_Skill01
        {
            get {
                if (m_DetailsPanel_Skill01 == null)
                {
                    m_DetailsPanel_Skill01 = contentPane.GetChildByPath("DetailsPanel.Skill01") as GComponent;
                }
                return m_DetailsPanel_Skill01;
            }
        }
        private GComponent m_DetailsPanel_Skill02;
        private GComponent DetailsPanel_Skill02
        {
            get {
                if (m_DetailsPanel_Skill02 == null)
                {
                    m_DetailsPanel_Skill02 = contentPane.GetChildByPath("DetailsPanel.Skill02") as GComponent;
                }
                return m_DetailsPanel_Skill02;
            }
        }
        private GComponent m_DetailsPanel_Skill03;
        private GComponent DetailsPanel_Skill03
        {
            get {
                if (m_DetailsPanel_Skill03 == null)
                {
                    m_DetailsPanel_Skill03 = contentPane.GetChildByPath("DetailsPanel.Skill03") as GComponent;
                }
                return m_DetailsPanel_Skill03;
            }
        }
        private GButton m_DetailsPanel_BtnMore;
        private GButton DetailsPanel_BtnMore
        {
            get {
                if (m_DetailsPanel_BtnMore == null)
                {
                    m_DetailsPanel_BtnMore = contentPane.GetChildByPath("DetailsPanel.BtnMore") as GButton;
                }
                return m_DetailsPanel_BtnMore;
            }
        }
        private GImage m_DetailsPanel_BtnMore_n0;
        private GImage DetailsPanel_BtnMore_n0
        {
            get {
                if (m_DetailsPanel_BtnMore_n0 == null)
                {
                    m_DetailsPanel_BtnMore_n0 = contentPane.GetChildByPath("DetailsPanel.BtnMore.n0") as GImage;
                }
                return m_DetailsPanel_BtnMore_n0;
            }
        }
        private GImage m_DetailsPanel_BtnMore_n3;
        private GImage DetailsPanel_BtnMore_n3
        {
            get {
                if (m_DetailsPanel_BtnMore_n3 == null)
                {
                    m_DetailsPanel_BtnMore_n3 = contentPane.GetChildByPath("DetailsPanel.BtnMore.n3") as GImage;
                }
                return m_DetailsPanel_BtnMore_n3;
            }
        }
        private GTextField m_DetailsPanel_BtnMore_title;
        private GTextField DetailsPanel_BtnMore_title
        {
            get {
                if (m_DetailsPanel_BtnMore_title == null)
                {
                    m_DetailsPanel_BtnMore_title = contentPane.GetChildByPath("DetailsPanel.BtnMore.title") as GTextField;
                }
                return m_DetailsPanel_BtnMore_title;
            }
        }
        private GTextField m_DetailsPanel_Text;
        private GTextField DetailsPanel_Text
        {
            get {
                if (m_DetailsPanel_Text == null)
                {
                    m_DetailsPanel_Text = contentPane.GetChildByPath("DetailsPanel.Text") as GTextField;
                }
                return m_DetailsPanel_Text;
            }
        }
        private GImage m_DetailsPanel_Bg;
        private GImage DetailsPanel_Bg
        {
            get {
                if (m_DetailsPanel_Bg == null)
                {
                    m_DetailsPanel_Bg = contentPane.GetChildByPath("DetailsPanel.Bg") as GImage;
                }
                return m_DetailsPanel_Bg;
            }
        }
        private GComponent m_DetailsPanel_BtnUpdate;
        private GComponent DetailsPanel_BtnUpdate
        {
            get {
                if (m_DetailsPanel_BtnUpdate == null)
                {
                    m_DetailsPanel_BtnUpdate = contentPane.GetChildByPath("DetailsPanel.BtnUpdate") as GComponent;
                }
                return m_DetailsPanel_BtnUpdate;
            }
        }
        private GComponent m_DetailsPanel_BtnAdvance;
        private GComponent DetailsPanel_BtnAdvance
        {
            get {
                if (m_DetailsPanel_BtnAdvance == null)
                {
                    m_DetailsPanel_BtnAdvance = contentPane.GetChildByPath("DetailsPanel.BtnAdvance") as GComponent;
                }
                return m_DetailsPanel_BtnAdvance;
            }
        }
        private GComponent m_DetailsPanel_CostItem01;
        private GComponent DetailsPanel_CostItem01
        {
            get {
                if (m_DetailsPanel_CostItem01 == null)
                {
                    m_DetailsPanel_CostItem01 = contentPane.GetChildByPath("DetailsPanel.CostItem01") as GComponent;
                }
                return m_DetailsPanel_CostItem01;
            }
        }
        private GComponent m_DetailsPanel_CostItem02;
        private GComponent DetailsPanel_CostItem02
        {
            get {
                if (m_DetailsPanel_CostItem02 == null)
                {
                    m_DetailsPanel_CostItem02 = contentPane.GetChildByPath("DetailsPanel.CostItem02") as GComponent;
                }
                return m_DetailsPanel_CostItem02;
            }
        }
        private GComponent m_StarPanel;
        private GComponent StarPanel
        {
            get {
                if (m_StarPanel == null)
                {
                    m_StarPanel = contentPane.GetChildByPath("StarPanel") as GComponent;
                }
                return m_StarPanel;
            }
        }
        private GImage m_StarPanel_Line;
        private GImage StarPanel_Line
        {
            get {
                if (m_StarPanel_Line == null)
                {
                    m_StarPanel_Line = contentPane.GetChildByPath("StarPanel.Line") as GImage;
                }
                return m_StarPanel_Line;
            }
        }
        private GImage m_StarPanel_Line3;
        private GImage StarPanel_Line3
        {
            get {
                if (m_StarPanel_Line3 == null)
                {
                    m_StarPanel_Line3 = contentPane.GetChildByPath("StarPanel.Line") as GImage;
                }
                return m_StarPanel_Line3;
            }
        }
        private GImage m_StarPanel_n161;
        private GImage StarPanel_n161
        {
            get {
                if (m_StarPanel_n161 == null)
                {
                    m_StarPanel_n161 = contentPane.GetChildByPath("StarPanel.n161") as GImage;
                }
                return m_StarPanel_n161;
            }
        }
        private GImage m_StarPanel_n162;
        private GImage StarPanel_n162
        {
            get {
                if (m_StarPanel_n162 == null)
                {
                    m_StarPanel_n162 = contentPane.GetChildByPath("StarPanel.n162") as GImage;
                }
                return m_StarPanel_n162;
            }
        }
        private GTextField m_StarPanel_TitleText;
        private GTextField StarPanel_TitleText
        {
            get {
                if (m_StarPanel_TitleText == null)
                {
                    m_StarPanel_TitleText = contentPane.GetChildByPath("StarPanel.TitleText") as GTextField;
                }
                return m_StarPanel_TitleText;
            }
        }
        private GComponent m_StarPanel_Skill01;
        private GComponent StarPanel_Skill01
        {
            get {
                if (m_StarPanel_Skill01 == null)
                {
                    m_StarPanel_Skill01 = contentPane.GetChildByPath("StarPanel.Skill01") as GComponent;
                }
                return m_StarPanel_Skill01;
            }
        }
        private GComponent m_StarPanel_Skill02;
        private GComponent StarPanel_Skill02
        {
            get {
                if (m_StarPanel_Skill02 == null)
                {
                    m_StarPanel_Skill02 = contentPane.GetChildByPath("StarPanel.Skill02") as GComponent;
                }
                return m_StarPanel_Skill02;
            }
        }
        private GComponent m_StarPanel_Skill03;
        private GComponent StarPanel_Skill03
        {
            get {
                if (m_StarPanel_Skill03 == null)
                {
                    m_StarPanel_Skill03 = contentPane.GetChildByPath("StarPanel.Skill03") as GComponent;
                }
                return m_StarPanel_Skill03;
            }
        }
        private GComponent m_StarPanel_Skill04;
        private GComponent StarPanel_Skill04
        {
            get {
                if (m_StarPanel_Skill04 == null)
                {
                    m_StarPanel_Skill04 = contentPane.GetChildByPath("StarPanel.Skill04") as GComponent;
                }
                return m_StarPanel_Skill04;
            }
        }
        private GComponent m_StarPanel_Skill05;
        private GComponent StarPanel_Skill05
        {
            get {
                if (m_StarPanel_Skill05 == null)
                {
                    m_StarPanel_Skill05 = contentPane.GetChildByPath("StarPanel.Skill05") as GComponent;
                }
                return m_StarPanel_Skill05;
            }
        }
        private GComponent m_StarPanel_Skill06;
        private GComponent StarPanel_Skill06
        {
            get {
                if (m_StarPanel_Skill06 == null)
                {
                    m_StarPanel_Skill06 = contentPane.GetChildByPath("StarPanel.Skill06") as GComponent;
                }
                return m_StarPanel_Skill06;
            }
        }
        private GComponent m_StarPanel_Skill07;
        private GComponent StarPanel_Skill07
        {
            get {
                if (m_StarPanel_Skill07 == null)
                {
                    m_StarPanel_Skill07 = contentPane.GetChildByPath("StarPanel.Skill07") as GComponent;
                }
                return m_StarPanel_Skill07;
            }
        }
        private GComponent m_StarPanel_CostItem;
        private GComponent StarPanel_CostItem
        {
            get {
                if (m_StarPanel_CostItem == null)
                {
                    m_StarPanel_CostItem = contentPane.GetChildByPath("StarPanel.CostItem") as GComponent;
                }
                return m_StarPanel_CostItem;
            }
        }
        private GComponent m_StarPanel_BtnUpdate;
        private GComponent StarPanel_BtnUpdate
        {
            get {
                if (m_StarPanel_BtnUpdate == null)
                {
                    m_StarPanel_BtnUpdate = contentPane.GetChildByPath("StarPanel.BtnUpdate") as GComponent;
                }
                return m_StarPanel_BtnUpdate;
            }
        }
        private GComponent m_EvolutionPanel;
        private GComponent EvolutionPanel
        {
            get {
                if (m_EvolutionPanel == null)
                {
                    m_EvolutionPanel = contentPane.GetChildByPath("EvolutionPanel") as GComponent;
                }
                return m_EvolutionPanel;
            }
        }
        private GImage m_EvolutionPanel_Line;
        private GImage EvolutionPanel_Line
        {
            get {
                if (m_EvolutionPanel_Line == null)
                {
                    m_EvolutionPanel_Line = contentPane.GetChildByPath("EvolutionPanel.Line") as GImage;
                }
                return m_EvolutionPanel_Line;
            }
        }
        private GImage m_EvolutionPanel_n163;
        private GImage EvolutionPanel_n163
        {
            get {
                if (m_EvolutionPanel_n163 == null)
                {
                    m_EvolutionPanel_n163 = contentPane.GetChildByPath("EvolutionPanel.n163") as GImage;
                }
                return m_EvolutionPanel_n163;
            }
        }
        private GImage m_EvolutionPanel_n164;
        private GImage EvolutionPanel_n164
        {
            get {
                if (m_EvolutionPanel_n164 == null)
                {
                    m_EvolutionPanel_n164 = contentPane.GetChildByPath("EvolutionPanel.n164") as GImage;
                }
                return m_EvolutionPanel_n164;
            }
        }
        private GTextField m_EvolutionPanel_TitleText;
        private GTextField EvolutionPanel_TitleText
        {
            get {
                if (m_EvolutionPanel_TitleText == null)
                {
                    m_EvolutionPanel_TitleText = contentPane.GetChildByPath("EvolutionPanel.TitleText") as GTextField;
                }
                return m_EvolutionPanel_TitleText;
            }
        }
        private GImage m_EvolutionPanel_Bg01;
        private GImage EvolutionPanel_Bg01
        {
            get {
                if (m_EvolutionPanel_Bg01 == null)
                {
                    m_EvolutionPanel_Bg01 = contentPane.GetChildByPath("EvolutionPanel.Bg01") as GImage;
                }
                return m_EvolutionPanel_Bg01;
            }
        }
        private GImage m_EvolutionPanel_Bg02;
        private GImage EvolutionPanel_Bg02
        {
            get {
                if (m_EvolutionPanel_Bg02 == null)
                {
                    m_EvolutionPanel_Bg02 = contentPane.GetChildByPath("EvolutionPanel.Bg02") as GImage;
                }
                return m_EvolutionPanel_Bg02;
            }
        }
        private GImage m_EvolutionPanel_Jiantou;
        private GImage EvolutionPanel_Jiantou
        {
            get {
                if (m_EvolutionPanel_Jiantou == null)
                {
                    m_EvolutionPanel_Jiantou = contentPane.GetChildByPath("EvolutionPanel.Jiantou") as GImage;
                }
                return m_EvolutionPanel_Jiantou;
            }
        }
        private GLoader m_EvolutionPanel_AptitudeIconNow;
        private GLoader EvolutionPanel_AptitudeIconNow
        {
            get {
                if (m_EvolutionPanel_AptitudeIconNow == null)
                {
                    m_EvolutionPanel_AptitudeIconNow = contentPane.GetChildByPath("EvolutionPanel.AptitudeIconNow") as GLoader;
                }
                return m_EvolutionPanel_AptitudeIconNow;
            }
        }
        private GLoader m_EvolutionPanel_AptitudeIconNext;
        private GLoader EvolutionPanel_AptitudeIconNext
        {
            get {
                if (m_EvolutionPanel_AptitudeIconNext == null)
                {
                    m_EvolutionPanel_AptitudeIconNext = contentPane.GetChildByPath("EvolutionPanel.AptitudeIconNext") as GLoader;
                }
                return m_EvolutionPanel_AptitudeIconNext;
            }
        }
        private GImage m_EvolutionPanel_StarDark;
        private GImage EvolutionPanel_StarDark
        {
            get {
                if (m_EvolutionPanel_StarDark == null)
                {
                    m_EvolutionPanel_StarDark = contentPane.GetChildByPath("EvolutionPanel.StarDark") as GImage;
                }
                return m_EvolutionPanel_StarDark;
            }
        }
        private GImage m_EvolutionPanel_StarLight;
        private GImage EvolutionPanel_StarLight
        {
            get {
                if (m_EvolutionPanel_StarLight == null)
                {
                    m_EvolutionPanel_StarLight = contentPane.GetChildByPath("EvolutionPanel.StarLight") as GImage;
                }
                return m_EvolutionPanel_StarLight;
            }
        }
        private GTextField m_EvolutionPanel_Text;
        private GTextField EvolutionPanel_Text
        {
            get {
                if (m_EvolutionPanel_Text == null)
                {
                    m_EvolutionPanel_Text = contentPane.GetChildByPath("EvolutionPanel.Text") as GTextField;
                }
                return m_EvolutionPanel_Text;
            }
        }
        private GTextField m_EvolutionPanel_Number;
        private GTextField EvolutionPanel_Number
        {
            get {
                if (m_EvolutionPanel_Number == null)
                {
                    m_EvolutionPanel_Number = contentPane.GetChildByPath("EvolutionPanel.Number") as GTextField;
                }
                return m_EvolutionPanel_Number;
            }
        }
        private GImage m_EvolutionPanel_Gou;
        private GImage EvolutionPanel_Gou
        {
            get {
                if (m_EvolutionPanel_Gou == null)
                {
                    m_EvolutionPanel_Gou = contentPane.GetChildByPath("EvolutionPanel.Gou") as GImage;
                }
                return m_EvolutionPanel_Gou;
            }
        }
        private GImage m_EvolutionPanel_StarDark4;
        private GImage EvolutionPanel_StarDark4
        {
            get {
                if (m_EvolutionPanel_StarDark4 == null)
                {
                    m_EvolutionPanel_StarDark4 = contentPane.GetChildByPath("EvolutionPanel.StarDark") as GImage;
                }
                return m_EvolutionPanel_StarDark4;
            }
        }
        private GImage m_EvolutionPanel_StarLight5;
        private GImage EvolutionPanel_StarLight5
        {
            get {
                if (m_EvolutionPanel_StarLight5 == null)
                {
                    m_EvolutionPanel_StarLight5 = contentPane.GetChildByPath("EvolutionPanel.StarLight") as GImage;
                }
                return m_EvolutionPanel_StarLight5;
            }
        }
        private GTextField m_EvolutionPanel_Text6;
        private GTextField EvolutionPanel_Text6
        {
            get {
                if (m_EvolutionPanel_Text6 == null)
                {
                    m_EvolutionPanel_Text6 = contentPane.GetChildByPath("EvolutionPanel.Text") as GTextField;
                }
                return m_EvolutionPanel_Text6;
            }
        }
        private GTextField m_EvolutionPanel_Number7;
        private GTextField EvolutionPanel_Number7
        {
            get {
                if (m_EvolutionPanel_Number7 == null)
                {
                    m_EvolutionPanel_Number7 = contentPane.GetChildByPath("EvolutionPanel.Number") as GTextField;
                }
                return m_EvolutionPanel_Number7;
            }
        }
        private GImage m_EvolutionPanel_Gou8;
        private GImage EvolutionPanel_Gou8
        {
            get {
                if (m_EvolutionPanel_Gou8 == null)
                {
                    m_EvolutionPanel_Gou8 = contentPane.GetChildByPath("EvolutionPanel.Gou") as GImage;
                }
                return m_EvolutionPanel_Gou8;
            }
        }
        private GImage m_EvolutionPanel_StarDark9;
        private GImage EvolutionPanel_StarDark9
        {
            get {
                if (m_EvolutionPanel_StarDark9 == null)
                {
                    m_EvolutionPanel_StarDark9 = contentPane.GetChildByPath("EvolutionPanel.StarDark") as GImage;
                }
                return m_EvolutionPanel_StarDark9;
            }
        }
        private GImage m_EvolutionPanel_StarLight10;
        private GImage EvolutionPanel_StarLight10
        {
            get {
                if (m_EvolutionPanel_StarLight10 == null)
                {
                    m_EvolutionPanel_StarLight10 = contentPane.GetChildByPath("EvolutionPanel.StarLight") as GImage;
                }
                return m_EvolutionPanel_StarLight10;
            }
        }
        private GTextField m_EvolutionPanel_Text11;
        private GTextField EvolutionPanel_Text11
        {
            get {
                if (m_EvolutionPanel_Text11 == null)
                {
                    m_EvolutionPanel_Text11 = contentPane.GetChildByPath("EvolutionPanel.Text") as GTextField;
                }
                return m_EvolutionPanel_Text11;
            }
        }
        private GTextField m_EvolutionPanel_Number12;
        private GTextField EvolutionPanel_Number12
        {
            get {
                if (m_EvolutionPanel_Number12 == null)
                {
                    m_EvolutionPanel_Number12 = contentPane.GetChildByPath("EvolutionPanel.Number") as GTextField;
                }
                return m_EvolutionPanel_Number12;
            }
        }
        private GImage m_EvolutionPanel_Gou13;
        private GImage EvolutionPanel_Gou13
        {
            get {
                if (m_EvolutionPanel_Gou13 == null)
                {
                    m_EvolutionPanel_Gou13 = contentPane.GetChildByPath("EvolutionPanel.Gou") as GImage;
                }
                return m_EvolutionPanel_Gou13;
            }
        }
        private GComponent m_EvolutionPanel_CostsItem01;
        private GComponent EvolutionPanel_CostsItem01
        {
            get {
                if (m_EvolutionPanel_CostsItem01 == null)
                {
                    m_EvolutionPanel_CostsItem01 = contentPane.GetChildByPath("EvolutionPanel.CostsItem01") as GComponent;
                }
                return m_EvolutionPanel_CostsItem01;
            }
        }
        private GComponent m_EvolutionPanel_BtnUpdate;
        private GComponent EvolutionPanel_BtnUpdate
        {
            get {
                if (m_EvolutionPanel_BtnUpdate == null)
                {
                    m_EvolutionPanel_BtnUpdate = contentPane.GetChildByPath("EvolutionPanel.BtnUpdate") as GComponent;
                }
                return m_EvolutionPanel_BtnUpdate;
            }
        }
        private GComponent m_AwakenPanel;
        private GComponent AwakenPanel
        {
            get {
                if (m_AwakenPanel == null)
                {
                    m_AwakenPanel = contentPane.GetChildByPath("AwakenPanel") as GComponent;
                }
                return m_AwakenPanel;
            }
        }
        private GImage m_AwakenPanel_Line;
        private GImage AwakenPanel_Line
        {
            get {
                if (m_AwakenPanel_Line == null)
                {
                    m_AwakenPanel_Line = contentPane.GetChildByPath("AwakenPanel.Line") as GImage;
                }
                return m_AwakenPanel_Line;
            }
        }
        private GImage m_AwakenPanel_n235;
        private GImage AwakenPanel_n235
        {
            get {
                if (m_AwakenPanel_n235 == null)
                {
                    m_AwakenPanel_n235 = contentPane.GetChildByPath("AwakenPanel.n235") as GImage;
                }
                return m_AwakenPanel_n235;
            }
        }
        private GImage m_AwakenPanel_n236;
        private GImage AwakenPanel_n236
        {
            get {
                if (m_AwakenPanel_n236 == null)
                {
                    m_AwakenPanel_n236 = contentPane.GetChildByPath("AwakenPanel.n236") as GImage;
                }
                return m_AwakenPanel_n236;
            }
        }
        private GTextField m_AwakenPanel_TitleText;
        private GTextField AwakenPanel_TitleText
        {
            get {
                if (m_AwakenPanel_TitleText == null)
                {
                    m_AwakenPanel_TitleText = contentPane.GetChildByPath("AwakenPanel.TitleText") as GTextField;
                }
                return m_AwakenPanel_TitleText;
            }
        }
        private GComponent m_AwakenPanel_Type1;
        private GComponent AwakenPanel_Type1
        {
            get {
                if (m_AwakenPanel_Type1 == null)
                {
                    m_AwakenPanel_Type1 = contentPane.GetChildByPath("AwakenPanel.Type1") as GComponent;
                }
                return m_AwakenPanel_Type1;
            }
        }
        private GImage m_AwakenPanel_Type1_Bg;
        private GImage AwakenPanel_Type1_Bg
        {
            get {
                if (m_AwakenPanel_Type1_Bg == null)
                {
                    m_AwakenPanel_Type1_Bg = contentPane.GetChildByPath("AwakenPanel.Type1.Bg") as GImage;
                }
                return m_AwakenPanel_Type1_Bg;
            }
        }
        private GImage m_AwakenPanel_Type1_Bg02;
        private GImage AwakenPanel_Type1_Bg02
        {
            get {
                if (m_AwakenPanel_Type1_Bg02 == null)
                {
                    m_AwakenPanel_Type1_Bg02 = contentPane.GetChildByPath("AwakenPanel.Type1.Bg02") as GImage;
                }
                return m_AwakenPanel_Type1_Bg02;
            }
        }
        private GImage m_AwakenPanel_Type1_n292;
        private GImage AwakenPanel_Type1_n292
        {
            get {
                if (m_AwakenPanel_Type1_n292 == null)
                {
                    m_AwakenPanel_Type1_n292 = contentPane.GetChildByPath("AwakenPanel.Type1.n292") as GImage;
                }
                return m_AwakenPanel_Type1_n292;
            }
        }
        private GImage m_AwakenPanel_Type1_StarLight;
        private GImage AwakenPanel_Type1_StarLight
        {
            get {
                if (m_AwakenPanel_Type1_StarLight == null)
                {
                    m_AwakenPanel_Type1_StarLight = contentPane.GetChildByPath("AwakenPanel.Type1.StarLight") as GImage;
                }
                return m_AwakenPanel_Type1_StarLight;
            }
        }
        private GTextField m_AwakenPanel_Type1_Text;
        private GTextField AwakenPanel_Type1_Text
        {
            get {
                if (m_AwakenPanel_Type1_Text == null)
                {
                    m_AwakenPanel_Type1_Text = contentPane.GetChildByPath("AwakenPanel.Type1.Text") as GTextField;
                }
                return m_AwakenPanel_Type1_Text;
            }
        }
        private GTextField m_AwakenPanel_Type1_DetailsText;
        private GTextField AwakenPanel_Type1_DetailsText
        {
            get {
                if (m_AwakenPanel_Type1_DetailsText == null)
                {
                    m_AwakenPanel_Type1_DetailsText = contentPane.GetChildByPath("AwakenPanel.Type1.DetailsText") as GTextField;
                }
                return m_AwakenPanel_Type1_DetailsText;
            }
        }
        private GComponent m_AwakenPanel_Type2;
        private GComponent AwakenPanel_Type2
        {
            get {
                if (m_AwakenPanel_Type2 == null)
                {
                    m_AwakenPanel_Type2 = contentPane.GetChildByPath("AwakenPanel.Type2") as GComponent;
                }
                return m_AwakenPanel_Type2;
            }
        }
        private GImage m_AwakenPanel_Type2_Bg;
        private GImage AwakenPanel_Type2_Bg
        {
            get {
                if (m_AwakenPanel_Type2_Bg == null)
                {
                    m_AwakenPanel_Type2_Bg = contentPane.GetChildByPath("AwakenPanel.Type2.Bg") as GImage;
                }
                return m_AwakenPanel_Type2_Bg;
            }
        }
        private GImage m_AwakenPanel_Type2_Bg02;
        private GImage AwakenPanel_Type2_Bg02
        {
            get {
                if (m_AwakenPanel_Type2_Bg02 == null)
                {
                    m_AwakenPanel_Type2_Bg02 = contentPane.GetChildByPath("AwakenPanel.Type2.Bg02") as GImage;
                }
                return m_AwakenPanel_Type2_Bg02;
            }
        }
        private GImage m_AwakenPanel_Type2_n292;
        private GImage AwakenPanel_Type2_n292
        {
            get {
                if (m_AwakenPanel_Type2_n292 == null)
                {
                    m_AwakenPanel_Type2_n292 = contentPane.GetChildByPath("AwakenPanel.Type2.n292") as GImage;
                }
                return m_AwakenPanel_Type2_n292;
            }
        }
        private GImage m_AwakenPanel_Type2_StarLight;
        private GImage AwakenPanel_Type2_StarLight
        {
            get {
                if (m_AwakenPanel_Type2_StarLight == null)
                {
                    m_AwakenPanel_Type2_StarLight = contentPane.GetChildByPath("AwakenPanel.Type2.StarLight") as GImage;
                }
                return m_AwakenPanel_Type2_StarLight;
            }
        }
        private GTextField m_AwakenPanel_Type2_Text;
        private GTextField AwakenPanel_Type2_Text
        {
            get {
                if (m_AwakenPanel_Type2_Text == null)
                {
                    m_AwakenPanel_Type2_Text = contentPane.GetChildByPath("AwakenPanel.Type2.Text") as GTextField;
                }
                return m_AwakenPanel_Type2_Text;
            }
        }
        private GTextField m_AwakenPanel_Type2_DetailsText;
        private GTextField AwakenPanel_Type2_DetailsText
        {
            get {
                if (m_AwakenPanel_Type2_DetailsText == null)
                {
                    m_AwakenPanel_Type2_DetailsText = contentPane.GetChildByPath("AwakenPanel.Type2.DetailsText") as GTextField;
                }
                return m_AwakenPanel_Type2_DetailsText;
            }
        }
        private GComponent m_AwakenPanel_Type3;
        private GComponent AwakenPanel_Type3
        {
            get {
                if (m_AwakenPanel_Type3 == null)
                {
                    m_AwakenPanel_Type3 = contentPane.GetChildByPath("AwakenPanel.Type3") as GComponent;
                }
                return m_AwakenPanel_Type3;
            }
        }
        private GImage m_AwakenPanel_Type3_Bg;
        private GImage AwakenPanel_Type3_Bg
        {
            get {
                if (m_AwakenPanel_Type3_Bg == null)
                {
                    m_AwakenPanel_Type3_Bg = contentPane.GetChildByPath("AwakenPanel.Type3.Bg") as GImage;
                }
                return m_AwakenPanel_Type3_Bg;
            }
        }
        private GImage m_AwakenPanel_Type3_Bg02;
        private GImage AwakenPanel_Type3_Bg02
        {
            get {
                if (m_AwakenPanel_Type3_Bg02 == null)
                {
                    m_AwakenPanel_Type3_Bg02 = contentPane.GetChildByPath("AwakenPanel.Type3.Bg02") as GImage;
                }
                return m_AwakenPanel_Type3_Bg02;
            }
        }
        private GImage m_AwakenPanel_Type3_n292;
        private GImage AwakenPanel_Type3_n292
        {
            get {
                if (m_AwakenPanel_Type3_n292 == null)
                {
                    m_AwakenPanel_Type3_n292 = contentPane.GetChildByPath("AwakenPanel.Type3.n292") as GImage;
                }
                return m_AwakenPanel_Type3_n292;
            }
        }
        private GImage m_AwakenPanel_Type3_StarLight;
        private GImage AwakenPanel_Type3_StarLight
        {
            get {
                if (m_AwakenPanel_Type3_StarLight == null)
                {
                    m_AwakenPanel_Type3_StarLight = contentPane.GetChildByPath("AwakenPanel.Type3.StarLight") as GImage;
                }
                return m_AwakenPanel_Type3_StarLight;
            }
        }
        private GTextField m_AwakenPanel_Type3_Text;
        private GTextField AwakenPanel_Type3_Text
        {
            get {
                if (m_AwakenPanel_Type3_Text == null)
                {
                    m_AwakenPanel_Type3_Text = contentPane.GetChildByPath("AwakenPanel.Type3.Text") as GTextField;
                }
                return m_AwakenPanel_Type3_Text;
            }
        }
        private GTextField m_AwakenPanel_Type3_DetailsText;
        private GTextField AwakenPanel_Type3_DetailsText
        {
            get {
                if (m_AwakenPanel_Type3_DetailsText == null)
                {
                    m_AwakenPanel_Type3_DetailsText = contentPane.GetChildByPath("AwakenPanel.Type3.DetailsText") as GTextField;
                }
                return m_AwakenPanel_Type3_DetailsText;
            }
        }
        private GComponent m_AwakenPanel_Type4;
        private GComponent AwakenPanel_Type4
        {
            get {
                if (m_AwakenPanel_Type4 == null)
                {
                    m_AwakenPanel_Type4 = contentPane.GetChildByPath("AwakenPanel.Type4") as GComponent;
                }
                return m_AwakenPanel_Type4;
            }
        }
        private GImage m_AwakenPanel_Type4_Bg;
        private GImage AwakenPanel_Type4_Bg
        {
            get {
                if (m_AwakenPanel_Type4_Bg == null)
                {
                    m_AwakenPanel_Type4_Bg = contentPane.GetChildByPath("AwakenPanel.Type4.Bg") as GImage;
                }
                return m_AwakenPanel_Type4_Bg;
            }
        }
        private GImage m_AwakenPanel_Type4_Bg02;
        private GImage AwakenPanel_Type4_Bg02
        {
            get {
                if (m_AwakenPanel_Type4_Bg02 == null)
                {
                    m_AwakenPanel_Type4_Bg02 = contentPane.GetChildByPath("AwakenPanel.Type4.Bg02") as GImage;
                }
                return m_AwakenPanel_Type4_Bg02;
            }
        }
        private GImage m_AwakenPanel_Type4_n292;
        private GImage AwakenPanel_Type4_n292
        {
            get {
                if (m_AwakenPanel_Type4_n292 == null)
                {
                    m_AwakenPanel_Type4_n292 = contentPane.GetChildByPath("AwakenPanel.Type4.n292") as GImage;
                }
                return m_AwakenPanel_Type4_n292;
            }
        }
        private GImage m_AwakenPanel_Type4_StarLight;
        private GImage AwakenPanel_Type4_StarLight
        {
            get {
                if (m_AwakenPanel_Type4_StarLight == null)
                {
                    m_AwakenPanel_Type4_StarLight = contentPane.GetChildByPath("AwakenPanel.Type4.StarLight") as GImage;
                }
                return m_AwakenPanel_Type4_StarLight;
            }
        }
        private GTextField m_AwakenPanel_Type4_Text;
        private GTextField AwakenPanel_Type4_Text
        {
            get {
                if (m_AwakenPanel_Type4_Text == null)
                {
                    m_AwakenPanel_Type4_Text = contentPane.GetChildByPath("AwakenPanel.Type4.Text") as GTextField;
                }
                return m_AwakenPanel_Type4_Text;
            }
        }
        private GTextField m_AwakenPanel_Type4_DetailsText;
        private GTextField AwakenPanel_Type4_DetailsText
        {
            get {
                if (m_AwakenPanel_Type4_DetailsText == null)
                {
                    m_AwakenPanel_Type4_DetailsText = contentPane.GetChildByPath("AwakenPanel.Type4.DetailsText") as GTextField;
                }
                return m_AwakenPanel_Type4_DetailsText;
            }
        }
        private GComponent m_AwakenPanel_Type5;
        private GComponent AwakenPanel_Type5
        {
            get {
                if (m_AwakenPanel_Type5 == null)
                {
                    m_AwakenPanel_Type5 = contentPane.GetChildByPath("AwakenPanel.Type5") as GComponent;
                }
                return m_AwakenPanel_Type5;
            }
        }
        private GImage m_AwakenPanel_Type5_Bg;
        private GImage AwakenPanel_Type5_Bg
        {
            get {
                if (m_AwakenPanel_Type5_Bg == null)
                {
                    m_AwakenPanel_Type5_Bg = contentPane.GetChildByPath("AwakenPanel.Type5.Bg") as GImage;
                }
                return m_AwakenPanel_Type5_Bg;
            }
        }
        private GImage m_AwakenPanel_Type5_Bg02;
        private GImage AwakenPanel_Type5_Bg02
        {
            get {
                if (m_AwakenPanel_Type5_Bg02 == null)
                {
                    m_AwakenPanel_Type5_Bg02 = contentPane.GetChildByPath("AwakenPanel.Type5.Bg02") as GImage;
                }
                return m_AwakenPanel_Type5_Bg02;
            }
        }
        private GImage m_AwakenPanel_Type5_n292;
        private GImage AwakenPanel_Type5_n292
        {
            get {
                if (m_AwakenPanel_Type5_n292 == null)
                {
                    m_AwakenPanel_Type5_n292 = contentPane.GetChildByPath("AwakenPanel.Type5.n292") as GImage;
                }
                return m_AwakenPanel_Type5_n292;
            }
        }
        private GImage m_AwakenPanel_Type5_StarLight;
        private GImage AwakenPanel_Type5_StarLight
        {
            get {
                if (m_AwakenPanel_Type5_StarLight == null)
                {
                    m_AwakenPanel_Type5_StarLight = contentPane.GetChildByPath("AwakenPanel.Type5.StarLight") as GImage;
                }
                return m_AwakenPanel_Type5_StarLight;
            }
        }
        private GTextField m_AwakenPanel_Type5_Text;
        private GTextField AwakenPanel_Type5_Text
        {
            get {
                if (m_AwakenPanel_Type5_Text == null)
                {
                    m_AwakenPanel_Type5_Text = contentPane.GetChildByPath("AwakenPanel.Type5.Text") as GTextField;
                }
                return m_AwakenPanel_Type5_Text;
            }
        }
        private GTextField m_AwakenPanel_Type5_DetailsText;
        private GTextField AwakenPanel_Type5_DetailsText
        {
            get {
                if (m_AwakenPanel_Type5_DetailsText == null)
                {
                    m_AwakenPanel_Type5_DetailsText = contentPane.GetChildByPath("AwakenPanel.Type5.DetailsText") as GTextField;
                }
                return m_AwakenPanel_Type5_DetailsText;
            }
        }
        private GComponent m_AwakenPanel_BtnUpdate;
        private GComponent AwakenPanel_BtnUpdate
        {
            get {
                if (m_AwakenPanel_BtnUpdate == null)
                {
                    m_AwakenPanel_BtnUpdate = contentPane.GetChildByPath("AwakenPanel.BtnUpdate") as GComponent;
                }
                return m_AwakenPanel_BtnUpdate;
            }
        }
        private GTextField m_AwakenPanel_Text;
        private GTextField AwakenPanel_Text
        {
            get {
                if (m_AwakenPanel_Text == null)
                {
                    m_AwakenPanel_Text = contentPane.GetChildByPath("AwakenPanel.Text") as GTextField;
                }
                return m_AwakenPanel_Text;
            }
        }
        private GImage m_AwakenPanel_Bg;
        private GImage AwakenPanel_Bg
        {
            get {
                if (m_AwakenPanel_Bg == null)
                {
                    m_AwakenPanel_Bg = contentPane.GetChildByPath("AwakenPanel.Bg") as GImage;
                }
                return m_AwakenPanel_Bg;
            }
        }
        private GTextField m_AwakenPanel_RemainTxt;
        private GTextField AwakenPanel_RemainTxt
        {
            get {
                if (m_AwakenPanel_RemainTxt == null)
                {
                    m_AwakenPanel_RemainTxt = contentPane.GetChildByPath("AwakenPanel.RemainTxt") as GTextField;
                }
                return m_AwakenPanel_RemainTxt;
            }
        }
        private GComponent m_AwakenPanel_CostItem;
        private GComponent AwakenPanel_CostItem
        {
            get {
                if (m_AwakenPanel_CostItem == null)
                {
                    m_AwakenPanel_CostItem = contentPane.GetChildByPath("AwakenPanel.CostItem") as GComponent;
                }
                return m_AwakenPanel_CostItem;
            }
        }
        private GComponent m_AwakenPanel_CostsItem01;
        private GComponent AwakenPanel_CostsItem01
        {
            get {
                if (m_AwakenPanel_CostsItem01 == null)
                {
                    m_AwakenPanel_CostsItem01 = contentPane.GetChildByPath("AwakenPanel.CostsItem01") as GComponent;
                }
                return m_AwakenPanel_CostsItem01;
            }
        }
        private GComponent m_LevelBtn;
        private GComponent LevelBtn
        {
            get {
                if (m_LevelBtn == null)
                {
                    m_LevelBtn = contentPane.GetChildByPath("LevelBtn") as GComponent;
                }
                return m_LevelBtn;
            }
        }
        private GComponent m_StarBtn;
        private GComponent StarBtn
        {
            get {
                if (m_StarBtn == null)
                {
                    m_StarBtn = contentPane.GetChildByPath("StarBtn") as GComponent;
                }
                return m_StarBtn;
            }
        }
        private GComponent m_EquipBtn;
        private GComponent EquipBtn
        {
            get {
                if (m_EquipBtn == null)
                {
                    m_EquipBtn = contentPane.GetChildByPath("EquipBtn") as GComponent;
                }
                return m_EquipBtn;
            }
        }
        private GButton m_CommonBtn;
        private GButton CommonBtn
        {
            get {
                if (m_CommonBtn == null)
                {
                    m_CommonBtn = contentPane.GetChildByPath("CommonBtn") as GButton;
                }
                return m_CommonBtn;
            }
        }
        private GImage m_CommonBtn_n1;
        private GImage CommonBtn_n1
        {
            get {
                if (m_CommonBtn_n1 == null)
                {
                    m_CommonBtn_n1 = contentPane.GetChildByPath("CommonBtn.n1") as GImage;
                }
                return m_CommonBtn_n1;
            }
        }
        private GImage m_CommonBtn_n0;
        private GImage CommonBtn_n0
        {
            get {
                if (m_CommonBtn_n0 == null)
                {
                    m_CommonBtn_n0 = contentPane.GetChildByPath("CommonBtn.n0") as GImage;
                }
                return m_CommonBtn_n0;
            }
        }
        private GComponent m_EquipPanel;
        private GComponent EquipPanel
        {
            get {
                if (m_EquipPanel == null)
                {
                    m_EquipPanel = contentPane.GetChildByPath("EquipPanel") as GComponent;
                }
                return m_EquipPanel;
            }
        }
        private GButton m_EquipPanel_BtnProperty;
        private GButton EquipPanel_BtnProperty
        {
            get {
                if (m_EquipPanel_BtnProperty == null)
                {
                    m_EquipPanel_BtnProperty = contentPane.GetChildByPath("EquipPanel.BtnProperty") as GButton;
                }
                return m_EquipPanel_BtnProperty;
            }
        }
        private GImage m_EquipPanel_BtnProperty_n0;
        private GImage EquipPanel_BtnProperty_n0
        {
            get {
                if (m_EquipPanel_BtnProperty_n0 == null)
                {
                    m_EquipPanel_BtnProperty_n0 = contentPane.GetChildByPath("EquipPanel.BtnProperty.n0") as GImage;
                }
                return m_EquipPanel_BtnProperty_n0;
            }
        }
        private GTextField m_EquipPanel_BtnProperty_n2;
        private GTextField EquipPanel_BtnProperty_n2
        {
            get {
                if (m_EquipPanel_BtnProperty_n2 == null)
                {
                    m_EquipPanel_BtnProperty_n2 = contentPane.GetChildByPath("EquipPanel.BtnProperty.n2") as GTextField;
                }
                return m_EquipPanel_BtnProperty_n2;
            }
        }
        private GButton m_EquipPanel_BtnResolve;
        private GButton EquipPanel_BtnResolve
        {
            get {
                if (m_EquipPanel_BtnResolve == null)
                {
                    m_EquipPanel_BtnResolve = contentPane.GetChildByPath("EquipPanel.BtnResolve") as GButton;
                }
                return m_EquipPanel_BtnResolve;
            }
        }
        private GImage m_EquipPanel_BtnResolve_n0;
        private GImage EquipPanel_BtnResolve_n0
        {
            get {
                if (m_EquipPanel_BtnResolve_n0 == null)
                {
                    m_EquipPanel_BtnResolve_n0 = contentPane.GetChildByPath("EquipPanel.BtnResolve.n0") as GImage;
                }
                return m_EquipPanel_BtnResolve_n0;
            }
        }
        private GTextField m_EquipPanel_BtnResolve_n2;
        private GTextField EquipPanel_BtnResolve_n2
        {
            get {
                if (m_EquipPanel_BtnResolve_n2 == null)
                {
                    m_EquipPanel_BtnResolve_n2 = contentPane.GetChildByPath("EquipPanel.BtnResolve.n2") as GTextField;
                }
                return m_EquipPanel_BtnResolve_n2;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame01;
        private GComponent EquipPanel_EquiptFrame01
        {
            get {
                if (m_EquipPanel_EquiptFrame01 == null)
                {
                    m_EquipPanel_EquiptFrame01 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01") as GComponent;
                }
                return m_EquipPanel_EquiptFrame01;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n39;
        private GImage EquipPanel_EquiptFrame01_n39
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n39 == null)
                {
                    m_EquipPanel_EquiptFrame01_n39 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n39") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n39;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n47;
        private GImage EquipPanel_EquiptFrame01_n47
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n47 == null)
                {
                    m_EquipPanel_EquiptFrame01_n47 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n47") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n47;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n34;
        private GImage EquipPanel_EquiptFrame01_n34
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n34 == null)
                {
                    m_EquipPanel_EquiptFrame01_n34 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n34") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n34;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n33;
        private GImage EquipPanel_EquiptFrame01_n33
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n33 == null)
                {
                    m_EquipPanel_EquiptFrame01_n33 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n33") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n33;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n35;
        private GImage EquipPanel_EquiptFrame01_n35
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n35 == null)
                {
                    m_EquipPanel_EquiptFrame01_n35 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n35") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n35;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n32;
        private GImage EquipPanel_EquiptFrame01_n32
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n32 == null)
                {
                    m_EquipPanel_EquiptFrame01_n32 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n32") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n32;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_n46;
        private GImage EquipPanel_EquiptFrame01_n46
        {
            get {
                if (m_EquipPanel_EquiptFrame01_n46 == null)
                {
                    m_EquipPanel_EquiptFrame01_n46 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.n46") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_n46;
            }
        }
        private GLoader m_EquipPanel_EquiptFrame01_EquiptIcon;
        private GLoader EquipPanel_EquiptFrame01_EquiptIcon
        {
            get {
                if (m_EquipPanel_EquiptFrame01_EquiptIcon == null)
                {
                    m_EquipPanel_EquiptFrame01_EquiptIcon = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.EquiptIcon") as GLoader;
                }
                return m_EquipPanel_EquiptFrame01_EquiptIcon;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_Bg;
        private GImage EquipPanel_EquiptFrame01_Bg
        {
            get {
                if (m_EquipPanel_EquiptFrame01_Bg == null)
                {
                    m_EquipPanel_EquiptFrame01_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_Bg;
            }
        }
        private GTextField m_EquipPanel_EquiptFrame01_NumberText;
        private GTextField EquipPanel_EquiptFrame01_NumberText
        {
            get {
                if (m_EquipPanel_EquiptFrame01_NumberText == null)
                {
                    m_EquipPanel_EquiptFrame01_NumberText = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.NumberText") as GTextField;
                }
                return m_EquipPanel_EquiptFrame01_NumberText;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_Jiezi;
        private GImage EquipPanel_EquiptFrame01_Jiezi
        {
            get {
                if (m_EquipPanel_EquiptFrame01_Jiezi == null)
                {
                    m_EquipPanel_EquiptFrame01_Jiezi = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.Jiezi") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_Jiezi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_Wuqi;
        private GImage EquipPanel_EquiptFrame01_Wuqi
        {
            get {
                if (m_EquipPanel_EquiptFrame01_Wuqi == null)
                {
                    m_EquipPanel_EquiptFrame01_Wuqi = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.Wuqi") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_Wuqi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_Xianglian;
        private GImage EquipPanel_EquiptFrame01_Xianglian
        {
            get {
                if (m_EquipPanel_EquiptFrame01_Xianglian == null)
                {
                    m_EquipPanel_EquiptFrame01_Xianglian = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.Xianglian") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_Xianglian;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_Yifu;
        private GImage EquipPanel_EquiptFrame01_Yifu
        {
            get {
                if (m_EquipPanel_EquiptFrame01_Yifu == null)
                {
                    m_EquipPanel_EquiptFrame01_Yifu = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.Yifu") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_Yifu;
            }
        }
        private GImage m_EquipPanel_EquiptFrame01_select;
        private GImage EquipPanel_EquiptFrame01_select
        {
            get {
                if (m_EquipPanel_EquiptFrame01_select == null)
                {
                    m_EquipPanel_EquiptFrame01_select = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.select") as GImage;
                }
                return m_EquipPanel_EquiptFrame01_select;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame01_eftuiequippanel001;
        private GComponent EquipPanel_EquiptFrame01_eftuiequippanel001
        {
            get {
                if (m_EquipPanel_EquiptFrame01_eftuiequippanel001 == null)
                {
                    m_EquipPanel_EquiptFrame01_eftuiequippanel001 = contentPane.GetChildByPath("EquipPanel.EquiptFrame01.eft_ui_equippanel_001") as GComponent;
                }
                return m_EquipPanel_EquiptFrame01_eftuiequippanel001;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame02;
        private GComponent EquipPanel_EquiptFrame02
        {
            get {
                if (m_EquipPanel_EquiptFrame02 == null)
                {
                    m_EquipPanel_EquiptFrame02 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02") as GComponent;
                }
                return m_EquipPanel_EquiptFrame02;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n39;
        private GImage EquipPanel_EquiptFrame02_n39
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n39 == null)
                {
                    m_EquipPanel_EquiptFrame02_n39 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n39") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n39;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n47;
        private GImage EquipPanel_EquiptFrame02_n47
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n47 == null)
                {
                    m_EquipPanel_EquiptFrame02_n47 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n47") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n47;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n34;
        private GImage EquipPanel_EquiptFrame02_n34
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n34 == null)
                {
                    m_EquipPanel_EquiptFrame02_n34 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n34") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n34;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n33;
        private GImage EquipPanel_EquiptFrame02_n33
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n33 == null)
                {
                    m_EquipPanel_EquiptFrame02_n33 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n33") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n33;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n35;
        private GImage EquipPanel_EquiptFrame02_n35
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n35 == null)
                {
                    m_EquipPanel_EquiptFrame02_n35 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n35") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n35;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n32;
        private GImage EquipPanel_EquiptFrame02_n32
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n32 == null)
                {
                    m_EquipPanel_EquiptFrame02_n32 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n32") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n32;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_n46;
        private GImage EquipPanel_EquiptFrame02_n46
        {
            get {
                if (m_EquipPanel_EquiptFrame02_n46 == null)
                {
                    m_EquipPanel_EquiptFrame02_n46 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.n46") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_n46;
            }
        }
        private GLoader m_EquipPanel_EquiptFrame02_EquiptIcon;
        private GLoader EquipPanel_EquiptFrame02_EquiptIcon
        {
            get {
                if (m_EquipPanel_EquiptFrame02_EquiptIcon == null)
                {
                    m_EquipPanel_EquiptFrame02_EquiptIcon = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.EquiptIcon") as GLoader;
                }
                return m_EquipPanel_EquiptFrame02_EquiptIcon;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_Bg;
        private GImage EquipPanel_EquiptFrame02_Bg
        {
            get {
                if (m_EquipPanel_EquiptFrame02_Bg == null)
                {
                    m_EquipPanel_EquiptFrame02_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_Bg;
            }
        }
        private GTextField m_EquipPanel_EquiptFrame02_NumberText;
        private GTextField EquipPanel_EquiptFrame02_NumberText
        {
            get {
                if (m_EquipPanel_EquiptFrame02_NumberText == null)
                {
                    m_EquipPanel_EquiptFrame02_NumberText = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.NumberText") as GTextField;
                }
                return m_EquipPanel_EquiptFrame02_NumberText;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_Jiezi;
        private GImage EquipPanel_EquiptFrame02_Jiezi
        {
            get {
                if (m_EquipPanel_EquiptFrame02_Jiezi == null)
                {
                    m_EquipPanel_EquiptFrame02_Jiezi = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.Jiezi") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_Jiezi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_Wuqi;
        private GImage EquipPanel_EquiptFrame02_Wuqi
        {
            get {
                if (m_EquipPanel_EquiptFrame02_Wuqi == null)
                {
                    m_EquipPanel_EquiptFrame02_Wuqi = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.Wuqi") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_Wuqi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_Xianglian;
        private GImage EquipPanel_EquiptFrame02_Xianglian
        {
            get {
                if (m_EquipPanel_EquiptFrame02_Xianglian == null)
                {
                    m_EquipPanel_EquiptFrame02_Xianglian = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.Xianglian") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_Xianglian;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_Yifu;
        private GImage EquipPanel_EquiptFrame02_Yifu
        {
            get {
                if (m_EquipPanel_EquiptFrame02_Yifu == null)
                {
                    m_EquipPanel_EquiptFrame02_Yifu = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.Yifu") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_Yifu;
            }
        }
        private GImage m_EquipPanel_EquiptFrame02_select;
        private GImage EquipPanel_EquiptFrame02_select
        {
            get {
                if (m_EquipPanel_EquiptFrame02_select == null)
                {
                    m_EquipPanel_EquiptFrame02_select = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.select") as GImage;
                }
                return m_EquipPanel_EquiptFrame02_select;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame02_eftuiequippanel001;
        private GComponent EquipPanel_EquiptFrame02_eftuiequippanel001
        {
            get {
                if (m_EquipPanel_EquiptFrame02_eftuiequippanel001 == null)
                {
                    m_EquipPanel_EquiptFrame02_eftuiequippanel001 = contentPane.GetChildByPath("EquipPanel.EquiptFrame02.eft_ui_equippanel_001") as GComponent;
                }
                return m_EquipPanel_EquiptFrame02_eftuiequippanel001;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame03;
        private GComponent EquipPanel_EquiptFrame03
        {
            get {
                if (m_EquipPanel_EquiptFrame03 == null)
                {
                    m_EquipPanel_EquiptFrame03 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03") as GComponent;
                }
                return m_EquipPanel_EquiptFrame03;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n39;
        private GImage EquipPanel_EquiptFrame03_n39
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n39 == null)
                {
                    m_EquipPanel_EquiptFrame03_n39 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n39") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n39;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n47;
        private GImage EquipPanel_EquiptFrame03_n47
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n47 == null)
                {
                    m_EquipPanel_EquiptFrame03_n47 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n47") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n47;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n34;
        private GImage EquipPanel_EquiptFrame03_n34
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n34 == null)
                {
                    m_EquipPanel_EquiptFrame03_n34 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n34") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n34;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n33;
        private GImage EquipPanel_EquiptFrame03_n33
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n33 == null)
                {
                    m_EquipPanel_EquiptFrame03_n33 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n33") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n33;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n35;
        private GImage EquipPanel_EquiptFrame03_n35
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n35 == null)
                {
                    m_EquipPanel_EquiptFrame03_n35 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n35") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n35;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n32;
        private GImage EquipPanel_EquiptFrame03_n32
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n32 == null)
                {
                    m_EquipPanel_EquiptFrame03_n32 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n32") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n32;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_n46;
        private GImage EquipPanel_EquiptFrame03_n46
        {
            get {
                if (m_EquipPanel_EquiptFrame03_n46 == null)
                {
                    m_EquipPanel_EquiptFrame03_n46 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.n46") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_n46;
            }
        }
        private GLoader m_EquipPanel_EquiptFrame03_EquiptIcon;
        private GLoader EquipPanel_EquiptFrame03_EquiptIcon
        {
            get {
                if (m_EquipPanel_EquiptFrame03_EquiptIcon == null)
                {
                    m_EquipPanel_EquiptFrame03_EquiptIcon = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.EquiptIcon") as GLoader;
                }
                return m_EquipPanel_EquiptFrame03_EquiptIcon;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_Bg;
        private GImage EquipPanel_EquiptFrame03_Bg
        {
            get {
                if (m_EquipPanel_EquiptFrame03_Bg == null)
                {
                    m_EquipPanel_EquiptFrame03_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_Bg;
            }
        }
        private GTextField m_EquipPanel_EquiptFrame03_NumberText;
        private GTextField EquipPanel_EquiptFrame03_NumberText
        {
            get {
                if (m_EquipPanel_EquiptFrame03_NumberText == null)
                {
                    m_EquipPanel_EquiptFrame03_NumberText = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.NumberText") as GTextField;
                }
                return m_EquipPanel_EquiptFrame03_NumberText;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_Jiezi;
        private GImage EquipPanel_EquiptFrame03_Jiezi
        {
            get {
                if (m_EquipPanel_EquiptFrame03_Jiezi == null)
                {
                    m_EquipPanel_EquiptFrame03_Jiezi = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.Jiezi") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_Jiezi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_Wuqi;
        private GImage EquipPanel_EquiptFrame03_Wuqi
        {
            get {
                if (m_EquipPanel_EquiptFrame03_Wuqi == null)
                {
                    m_EquipPanel_EquiptFrame03_Wuqi = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.Wuqi") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_Wuqi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_Xianglian;
        private GImage EquipPanel_EquiptFrame03_Xianglian
        {
            get {
                if (m_EquipPanel_EquiptFrame03_Xianglian == null)
                {
                    m_EquipPanel_EquiptFrame03_Xianglian = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.Xianglian") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_Xianglian;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_Yifu;
        private GImage EquipPanel_EquiptFrame03_Yifu
        {
            get {
                if (m_EquipPanel_EquiptFrame03_Yifu == null)
                {
                    m_EquipPanel_EquiptFrame03_Yifu = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.Yifu") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_Yifu;
            }
        }
        private GImage m_EquipPanel_EquiptFrame03_select;
        private GImage EquipPanel_EquiptFrame03_select
        {
            get {
                if (m_EquipPanel_EquiptFrame03_select == null)
                {
                    m_EquipPanel_EquiptFrame03_select = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.select") as GImage;
                }
                return m_EquipPanel_EquiptFrame03_select;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame03_eftuiequippanel001;
        private GComponent EquipPanel_EquiptFrame03_eftuiequippanel001
        {
            get {
                if (m_EquipPanel_EquiptFrame03_eftuiequippanel001 == null)
                {
                    m_EquipPanel_EquiptFrame03_eftuiequippanel001 = contentPane.GetChildByPath("EquipPanel.EquiptFrame03.eft_ui_equippanel_001") as GComponent;
                }
                return m_EquipPanel_EquiptFrame03_eftuiequippanel001;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame04;
        private GComponent EquipPanel_EquiptFrame04
        {
            get {
                if (m_EquipPanel_EquiptFrame04 == null)
                {
                    m_EquipPanel_EquiptFrame04 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04") as GComponent;
                }
                return m_EquipPanel_EquiptFrame04;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n39;
        private GImage EquipPanel_EquiptFrame04_n39
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n39 == null)
                {
                    m_EquipPanel_EquiptFrame04_n39 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n39") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n39;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n47;
        private GImage EquipPanel_EquiptFrame04_n47
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n47 == null)
                {
                    m_EquipPanel_EquiptFrame04_n47 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n47") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n47;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n34;
        private GImage EquipPanel_EquiptFrame04_n34
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n34 == null)
                {
                    m_EquipPanel_EquiptFrame04_n34 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n34") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n34;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n33;
        private GImage EquipPanel_EquiptFrame04_n33
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n33 == null)
                {
                    m_EquipPanel_EquiptFrame04_n33 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n33") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n33;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n35;
        private GImage EquipPanel_EquiptFrame04_n35
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n35 == null)
                {
                    m_EquipPanel_EquiptFrame04_n35 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n35") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n35;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n32;
        private GImage EquipPanel_EquiptFrame04_n32
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n32 == null)
                {
                    m_EquipPanel_EquiptFrame04_n32 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n32") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n32;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_n46;
        private GImage EquipPanel_EquiptFrame04_n46
        {
            get {
                if (m_EquipPanel_EquiptFrame04_n46 == null)
                {
                    m_EquipPanel_EquiptFrame04_n46 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.n46") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_n46;
            }
        }
        private GLoader m_EquipPanel_EquiptFrame04_EquiptIcon;
        private GLoader EquipPanel_EquiptFrame04_EquiptIcon
        {
            get {
                if (m_EquipPanel_EquiptFrame04_EquiptIcon == null)
                {
                    m_EquipPanel_EquiptFrame04_EquiptIcon = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.EquiptIcon") as GLoader;
                }
                return m_EquipPanel_EquiptFrame04_EquiptIcon;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_Bg;
        private GImage EquipPanel_EquiptFrame04_Bg
        {
            get {
                if (m_EquipPanel_EquiptFrame04_Bg == null)
                {
                    m_EquipPanel_EquiptFrame04_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_Bg;
            }
        }
        private GTextField m_EquipPanel_EquiptFrame04_NumberText;
        private GTextField EquipPanel_EquiptFrame04_NumberText
        {
            get {
                if (m_EquipPanel_EquiptFrame04_NumberText == null)
                {
                    m_EquipPanel_EquiptFrame04_NumberText = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.NumberText") as GTextField;
                }
                return m_EquipPanel_EquiptFrame04_NumberText;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_Jiezi;
        private GImage EquipPanel_EquiptFrame04_Jiezi
        {
            get {
                if (m_EquipPanel_EquiptFrame04_Jiezi == null)
                {
                    m_EquipPanel_EquiptFrame04_Jiezi = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.Jiezi") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_Jiezi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_Wuqi;
        private GImage EquipPanel_EquiptFrame04_Wuqi
        {
            get {
                if (m_EquipPanel_EquiptFrame04_Wuqi == null)
                {
                    m_EquipPanel_EquiptFrame04_Wuqi = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.Wuqi") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_Wuqi;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_Xianglian;
        private GImage EquipPanel_EquiptFrame04_Xianglian
        {
            get {
                if (m_EquipPanel_EquiptFrame04_Xianglian == null)
                {
                    m_EquipPanel_EquiptFrame04_Xianglian = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.Xianglian") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_Xianglian;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_Yifu;
        private GImage EquipPanel_EquiptFrame04_Yifu
        {
            get {
                if (m_EquipPanel_EquiptFrame04_Yifu == null)
                {
                    m_EquipPanel_EquiptFrame04_Yifu = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.Yifu") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_Yifu;
            }
        }
        private GImage m_EquipPanel_EquiptFrame04_select;
        private GImage EquipPanel_EquiptFrame04_select
        {
            get {
                if (m_EquipPanel_EquiptFrame04_select == null)
                {
                    m_EquipPanel_EquiptFrame04_select = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.select") as GImage;
                }
                return m_EquipPanel_EquiptFrame04_select;
            }
        }
        private GComponent m_EquipPanel_EquiptFrame04_eftuiequippanel001;
        private GComponent EquipPanel_EquiptFrame04_eftuiequippanel001
        {
            get {
                if (m_EquipPanel_EquiptFrame04_eftuiequippanel001 == null)
                {
                    m_EquipPanel_EquiptFrame04_eftuiequippanel001 = contentPane.GetChildByPath("EquipPanel.EquiptFrame04.eft_ui_equippanel_001") as GComponent;
                }
                return m_EquipPanel_EquiptFrame04_eftuiequippanel001;
            }
        }
        private GLoader m_EquipPanel_Mask;
        private GLoader EquipPanel_Mask
        {
            get {
                if (m_EquipPanel_Mask == null)
                {
                    m_EquipPanel_Mask = contentPane.GetChildByPath("EquipPanel.Mask") as GLoader;
                }
                return m_EquipPanel_Mask;
            }
        }
        private GImage m_EquipPanel_Line;
        private GImage EquipPanel_Line
        {
            get {
                if (m_EquipPanel_Line == null)
                {
                    m_EquipPanel_Line = contentPane.GetChildByPath("EquipPanel.Line") as GImage;
                }
                return m_EquipPanel_Line;
            }
        }
        private GImage m_EquipPanel_Bg;
        private GImage EquipPanel_Bg
        {
            get {
                if (m_EquipPanel_Bg == null)
                {
                    m_EquipPanel_Bg = contentPane.GetChildByPath("EquipPanel.Bg") as GImage;
                }
                return m_EquipPanel_Bg;
            }
        }
        private GButton m_EquipPanel_EquiptFilterBtn01;
        private GButton EquipPanel_EquiptFilterBtn01
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn01 == null)
                {
                    m_EquipPanel_EquiptFilterBtn01 = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn01") as GButton;
                }
                return m_EquipPanel_EquiptFilterBtn01;
            }
        }
        private GImage m_EquipPanel_EquiptFilterBtn01_Bg;
        private GImage EquipPanel_EquiptFilterBtn01_Bg
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn01_Bg == null)
                {
                    m_EquipPanel_EquiptFilterBtn01_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn01.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFilterBtn01_Bg;
            }
        }
        private GLoader m_EquipPanel_EquiptFilterBtn01_icon;
        private GLoader EquipPanel_EquiptFilterBtn01_icon
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn01_icon == null)
                {
                    m_EquipPanel_EquiptFilterBtn01_icon = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn01.icon") as GLoader;
                }
                return m_EquipPanel_EquiptFilterBtn01_icon;
            }
        }
        private GButton m_EquipPanel_EquiptFilterBtn02;
        private GButton EquipPanel_EquiptFilterBtn02
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn02 == null)
                {
                    m_EquipPanel_EquiptFilterBtn02 = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn02") as GButton;
                }
                return m_EquipPanel_EquiptFilterBtn02;
            }
        }
        private GImage m_EquipPanel_EquiptFilterBtn02_Bg;
        private GImage EquipPanel_EquiptFilterBtn02_Bg
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn02_Bg == null)
                {
                    m_EquipPanel_EquiptFilterBtn02_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn02.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFilterBtn02_Bg;
            }
        }
        private GLoader m_EquipPanel_EquiptFilterBtn02_icon;
        private GLoader EquipPanel_EquiptFilterBtn02_icon
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn02_icon == null)
                {
                    m_EquipPanel_EquiptFilterBtn02_icon = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn02.icon") as GLoader;
                }
                return m_EquipPanel_EquiptFilterBtn02_icon;
            }
        }
        private GButton m_EquipPanel_EquiptFilterBtn03;
        private GButton EquipPanel_EquiptFilterBtn03
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn03 == null)
                {
                    m_EquipPanel_EquiptFilterBtn03 = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn03") as GButton;
                }
                return m_EquipPanel_EquiptFilterBtn03;
            }
        }
        private GImage m_EquipPanel_EquiptFilterBtn03_Bg;
        private GImage EquipPanel_EquiptFilterBtn03_Bg
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn03_Bg == null)
                {
                    m_EquipPanel_EquiptFilterBtn03_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn03.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFilterBtn03_Bg;
            }
        }
        private GLoader m_EquipPanel_EquiptFilterBtn03_icon;
        private GLoader EquipPanel_EquiptFilterBtn03_icon
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn03_icon == null)
                {
                    m_EquipPanel_EquiptFilterBtn03_icon = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn03.icon") as GLoader;
                }
                return m_EquipPanel_EquiptFilterBtn03_icon;
            }
        }
        private GButton m_EquipPanel_EquiptFilterBtn04;
        private GButton EquipPanel_EquiptFilterBtn04
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn04 == null)
                {
                    m_EquipPanel_EquiptFilterBtn04 = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn04") as GButton;
                }
                return m_EquipPanel_EquiptFilterBtn04;
            }
        }
        private GImage m_EquipPanel_EquiptFilterBtn04_Bg;
        private GImage EquipPanel_EquiptFilterBtn04_Bg
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn04_Bg == null)
                {
                    m_EquipPanel_EquiptFilterBtn04_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn04.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFilterBtn04_Bg;
            }
        }
        private GLoader m_EquipPanel_EquiptFilterBtn04_icon;
        private GLoader EquipPanel_EquiptFilterBtn04_icon
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn04_icon == null)
                {
                    m_EquipPanel_EquiptFilterBtn04_icon = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn04.icon") as GLoader;
                }
                return m_EquipPanel_EquiptFilterBtn04_icon;
            }
        }
        private GButton m_EquipPanel_EquiptFilterBtn05;
        private GButton EquipPanel_EquiptFilterBtn05
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn05 == null)
                {
                    m_EquipPanel_EquiptFilterBtn05 = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn05") as GButton;
                }
                return m_EquipPanel_EquiptFilterBtn05;
            }
        }
        private GImage m_EquipPanel_EquiptFilterBtn05_Bg;
        private GImage EquipPanel_EquiptFilterBtn05_Bg
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn05_Bg == null)
                {
                    m_EquipPanel_EquiptFilterBtn05_Bg = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn05.Bg") as GImage;
                }
                return m_EquipPanel_EquiptFilterBtn05_Bg;
            }
        }
        private GLoader m_EquipPanel_EquiptFilterBtn05_icon;
        private GLoader EquipPanel_EquiptFilterBtn05_icon
        {
            get {
                if (m_EquipPanel_EquiptFilterBtn05_icon == null)
                {
                    m_EquipPanel_EquiptFilterBtn05_icon = contentPane.GetChildByPath("EquipPanel.EquiptFilterBtn05.icon") as GLoader;
                }
                return m_EquipPanel_EquiptFilterBtn05_icon;
            }
        }
        private GButton m_EquipPanel_BtnEquiptFilter;
        private GButton EquipPanel_BtnEquiptFilter
        {
            get {
                if (m_EquipPanel_BtnEquiptFilter == null)
                {
                    m_EquipPanel_BtnEquiptFilter = contentPane.GetChildByPath("EquipPanel.BtnEquiptFilter") as GButton;
                }
                return m_EquipPanel_BtnEquiptFilter;
            }
        }
        private GImage m_EquipPanel_BtnEquiptFilter_Bg;
        private GImage EquipPanel_BtnEquiptFilter_Bg
        {
            get {
                if (m_EquipPanel_BtnEquiptFilter_Bg == null)
                {
                    m_EquipPanel_BtnEquiptFilter_Bg = contentPane.GetChildByPath("EquipPanel.BtnEquiptFilter.Bg") as GImage;
                }
                return m_EquipPanel_BtnEquiptFilter_Bg;
            }
        }
        private GTextField m_EquipPanel_BtnEquiptFilter_Text;
        private GTextField EquipPanel_BtnEquiptFilter_Text
        {
            get {
                if (m_EquipPanel_BtnEquiptFilter_Text == null)
                {
                    m_EquipPanel_BtnEquiptFilter_Text = contentPane.GetChildByPath("EquipPanel.BtnEquiptFilter.Text") as GTextField;
                }
                return m_EquipPanel_BtnEquiptFilter_Text;
            }
        }
        private GImage m_EquipPanel_BtnEquiptFilter_FilterIcon;
        private GImage EquipPanel_BtnEquiptFilter_FilterIcon
        {
            get {
                if (m_EquipPanel_BtnEquiptFilter_FilterIcon == null)
                {
                    m_EquipPanel_BtnEquiptFilter_FilterIcon = contentPane.GetChildByPath("EquipPanel.BtnEquiptFilter.FilterIcon") as GImage;
                }
                return m_EquipPanel_BtnEquiptFilter_FilterIcon;
            }
        }
        private GList m_EquipPanel_ItemFrameGroup;
        private GList EquipPanel_ItemFrameGroup
        {
            get {
                if (m_EquipPanel_ItemFrameGroup == null)
                {
                    m_EquipPanel_ItemFrameGroup = contentPane.GetChildByPath("EquipPanel.ItemFrameGroup") as GList;
                }
                return m_EquipPanel_ItemFrameGroup;
            }
        }
        private GComponent m_EquipPanel_DressBtn;
        private GComponent EquipPanel_DressBtn
        {
            get {
                if (m_EquipPanel_DressBtn == null)
                {
                    m_EquipPanel_DressBtn = contentPane.GetChildByPath("EquipPanel.DressBtn") as GComponent;
                }
                return m_EquipPanel_DressBtn;
            }
        }
        private GComponent m_EquipPanel_UndressBtn;
        private GComponent EquipPanel_UndressBtn
        {
            get {
                if (m_EquipPanel_UndressBtn == null)
                {
                    m_EquipPanel_UndressBtn = contentPane.GetChildByPath("EquipPanel.UndressBtn") as GComponent;
                }
                return m_EquipPanel_UndressBtn;
            }
        }
        private GComponent m_EquipPanel_DontStrengthenBtn;
        private GComponent EquipPanel_DontStrengthenBtn
        {
            get {
                if (m_EquipPanel_DontStrengthenBtn == null)
                {
                    m_EquipPanel_DontStrengthenBtn = contentPane.GetChildByPath("EquipPanel.DontStrengthenBtn") as GComponent;
                }
                return m_EquipPanel_DontStrengthenBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips;
        private GComponent EquipPanel_EquiptStrengthenTips
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips == null)
                {
                    m_EquipPanel_EquiptStrengthenTips = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_TipsBg;
        private GImage EquipPanel_EquiptStrengthenTips_TipsBg
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_TipsBg == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_TipsBg = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.TipsBg") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_TipsBg;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_Line;
        private GImage EquipPanel_EquiptStrengthenTips_Line
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Line == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Line = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Line") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_Line;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_n67;
        private GImage EquipPanel_EquiptStrengthenTips_n67
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_n67 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_n67 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.n67") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_n67;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_n68;
        private GImage EquipPanel_EquiptStrengthenTips_n68
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_n68 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_n68 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.n68") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_n68;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_TitleText;
        private GTextField EquipPanel_EquiptStrengthenTips_TitleText
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_TitleText == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_TitleText = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.TitleText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_TitleText;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_Line14;
        private GImage EquipPanel_EquiptStrengthenTips_Line14
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Line14 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Line14 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Line") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_Line14;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_n72;
        private GImage EquipPanel_EquiptStrengthenTips_n72
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_n72 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_n72 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.n72") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_n72;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_n73;
        private GImage EquipPanel_EquiptStrengthenTips_n73
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_n73 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_n73 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.n73") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_n73;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_TitleText15;
        private GTextField EquipPanel_EquiptStrengthenTips_TitleText15
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_TitleText15 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_TitleText15 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.TitleText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_TitleText15;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_Line01;
        private GImage EquipPanel_EquiptStrengthenTips_Line01
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Line01 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Line01 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Line01") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_Line01;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_EquiptFrame;
        private GComponent EquipPanel_EquiptStrengthenTips_EquiptFrame
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_EquiptFrame == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_EquiptFrame = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.EquiptFrame") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_EquiptFrame;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_EquiptName;
        private GTextField EquipPanel_EquiptStrengthenTips_EquiptName
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_EquiptName == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_EquiptName = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.EquiptName") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_EquiptName;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_CardDressText;
        private GTextField EquipPanel_EquiptStrengthenTips_CardDressText
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_CardDressText == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_CardDressText = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.CardDressText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_CardDressText;
            }
        }
        private GLoader m_EquipPanel_EquiptStrengthenTips_Icon;
        private GLoader EquipPanel_EquiptStrengthenTips_Icon
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Icon == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Icon = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Icon") as GLoader;
                }
                return m_EquipPanel_EquiptStrengthenTips_Icon;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_PropertyText;
        private GTextField EquipPanel_EquiptStrengthenTips_PropertyText
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_PropertyText == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_PropertyText = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.PropertyText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_PropertyText;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_NumberText;
        private GTextField EquipPanel_EquiptStrengthenTips_NumberText
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_NumberText == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_NumberText = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.NumberText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_NumberText;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_NextNumberText;
        private GTextField EquipPanel_EquiptStrengthenTips_NextNumberText
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_NextNumberText == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_NextNumberText = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.NextNumberText") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_NextNumberText;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_BlueArrows;
        private GImage EquipPanel_EquiptStrengthenTips_BlueArrows
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_BlueArrows == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_BlueArrows = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.BlueArrows") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_BlueArrows;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_UpArrows;
        private GImage EquipPanel_EquiptStrengthenTips_UpArrows
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_UpArrows == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_UpArrows = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.UpArrows") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_UpArrows;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_StrengthenBtn;
        private GComponent EquipPanel_EquiptStrengthenTips_StrengthenBtn
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_StrengthenBtn == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_StrengthenBtn = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.StrengthenBtn") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_StrengthenBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem01;
        private GComponent EquipPanel_EquiptStrengthenTips_RandomPropertyItem01
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem01 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem01 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.RandomPropertyItem01") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem01;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem02;
        private GComponent EquipPanel_EquiptStrengthenTips_RandomPropertyItem02
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem02 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem02 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.RandomPropertyItem02") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem02;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem03;
        private GComponent EquipPanel_EquiptStrengthenTips_RandomPropertyItem03
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem03 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem03 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.RandomPropertyItem03") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem03;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem04;
        private GComponent EquipPanel_EquiptStrengthenTips_RandomPropertyItem04
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem04 == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem04 = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.RandomPropertyItem04") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_RandomPropertyItem04;
            }
        }
        private GComponent m_EquipPanel_EquiptStrengthenTips_Cost;
        private GComponent EquipPanel_EquiptStrengthenTips_Cost
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Cost == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Cost = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Cost") as GComponent;
                }
                return m_EquipPanel_EquiptStrengthenTips_Cost;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_Img;
        private GImage EquipPanel_EquiptStrengthenTips_Img
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Img == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Img = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Img") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_Img;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_NotShow;
        private GTextField EquipPanel_EquiptStrengthenTips_NotShow
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_NotShow == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_NotShow = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.NotShow") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_NotShow;
            }
        }
        private GTextField m_EquipPanel_EquiptStrengthenTips_Limit;
        private GTextField EquipPanel_EquiptStrengthenTips_Limit
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Limit == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Limit = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Limit") as GTextField;
                }
                return m_EquipPanel_EquiptStrengthenTips_Limit;
            }
        }
        private GImage m_EquipPanel_EquiptStrengthenTips_Bg;
        private GImage EquipPanel_EquiptStrengthenTips_Bg
        {
            get {
                if (m_EquipPanel_EquiptStrengthenTips_Bg == null)
                {
                    m_EquipPanel_EquiptStrengthenTips_Bg = contentPane.GetChildByPath("EquipPanel.EquiptStrengthenTips.Bg") as GImage;
                }
                return m_EquipPanel_EquiptStrengthenTips_Bg;
            }
        }
        private GComponent m_EquipPanel_EquiptResolveTips;
        private GComponent EquipPanel_EquiptResolveTips
        {
            get {
                if (m_EquipPanel_EquiptResolveTips == null)
                {
                    m_EquipPanel_EquiptResolveTips = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips") as GComponent;
                }
                return m_EquipPanel_EquiptResolveTips;
            }
        }
        private GImage m_EquipPanel_EquiptResolveTips_TipsBg;
        private GImage EquipPanel_EquiptResolveTips_TipsBg
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_TipsBg == null)
                {
                    m_EquipPanel_EquiptResolveTips_TipsBg = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.TipsBg") as GImage;
                }
                return m_EquipPanel_EquiptResolveTips_TipsBg;
            }
        }
        private GImage m_EquipPanel_EquiptResolveTips_n68;
        private GImage EquipPanel_EquiptResolveTips_n68
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_n68 == null)
                {
                    m_EquipPanel_EquiptResolveTips_n68 = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.n68") as GImage;
                }
                return m_EquipPanel_EquiptResolveTips_n68;
            }
        }
        private GImage m_EquipPanel_EquiptResolveTips_n69;
        private GImage EquipPanel_EquiptResolveTips_n69
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_n69 == null)
                {
                    m_EquipPanel_EquiptResolveTips_n69 = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.n69") as GImage;
                }
                return m_EquipPanel_EquiptResolveTips_n69;
            }
        }
        private GTextField m_EquipPanel_EquiptResolveTips_TitleText;
        private GTextField EquipPanel_EquiptResolveTips_TitleText
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_TitleText == null)
                {
                    m_EquipPanel_EquiptResolveTips_TitleText = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.TitleText") as GTextField;
                }
                return m_EquipPanel_EquiptResolveTips_TitleText;
            }
        }
        private GImage m_EquipPanel_EquiptResolveTips_Line02;
        private GImage EquipPanel_EquiptResolveTips_Line02
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_Line02 == null)
                {
                    m_EquipPanel_EquiptResolveTips_Line02 = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.Line02") as GImage;
                }
                return m_EquipPanel_EquiptResolveTips_Line02;
            }
        }
        private GList m_EquipPanel_EquiptResolveTips_ItemFrameGroup;
        private GList EquipPanel_EquiptResolveTips_ItemFrameGroup
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_ItemFrameGroup == null)
                {
                    m_EquipPanel_EquiptResolveTips_ItemFrameGroup = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.ItemFrameGroup") as GList;
                }
                return m_EquipPanel_EquiptResolveTips_ItemFrameGroup;
            }
        }
        private GTextField m_EquipPanel_EquiptResolveTips_Text;
        private GTextField EquipPanel_EquiptResolveTips_Text
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_Text == null)
                {
                    m_EquipPanel_EquiptResolveTips_Text = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.Text") as GTextField;
                }
                return m_EquipPanel_EquiptResolveTips_Text;
            }
        }
        private GComponent m_EquipPanel_EquiptResolveTips_EmptyBtn;
        private GComponent EquipPanel_EquiptResolveTips_EmptyBtn
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_EmptyBtn == null)
                {
                    m_EquipPanel_EquiptResolveTips_EmptyBtn = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.EmptyBtn") as GComponent;
                }
                return m_EquipPanel_EquiptResolveTips_EmptyBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptResolveTips_ResolveBtn;
        private GComponent EquipPanel_EquiptResolveTips_ResolveBtn
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_ResolveBtn == null)
                {
                    m_EquipPanel_EquiptResolveTips_ResolveBtn = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.ResolveBtn") as GComponent;
                }
                return m_EquipPanel_EquiptResolveTips_ResolveBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptResolveTips_Cost;
        private GComponent EquipPanel_EquiptResolveTips_Cost
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_Cost == null)
                {
                    m_EquipPanel_EquiptResolveTips_Cost = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.Cost") as GComponent;
                }
                return m_EquipPanel_EquiptResolveTips_Cost;
            }
        }
        private GImage m_EquipPanel_EquiptResolveTips_Img;
        private GImage EquipPanel_EquiptResolveTips_Img
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_Img == null)
                {
                    m_EquipPanel_EquiptResolveTips_Img = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.Img") as GImage;
                }
                return m_EquipPanel_EquiptResolveTips_Img;
            }
        }
        private GTextField m_EquipPanel_EquiptResolveTips_NotShow;
        private GTextField EquipPanel_EquiptResolveTips_NotShow
        {
            get {
                if (m_EquipPanel_EquiptResolveTips_NotShow == null)
                {
                    m_EquipPanel_EquiptResolveTips_NotShow = contentPane.GetChildByPath("EquipPanel.EquiptResolveTips.NotShow") as GTextField;
                }
                return m_EquipPanel_EquiptResolveTips_NotShow;
            }
        }
        private GComponent m_EquipPanel_EquiptDetailsTips;
        private GComponent EquipPanel_EquiptDetailsTips
        {
            get {
                if (m_EquipPanel_EquiptDetailsTips == null)
                {
                    m_EquipPanel_EquiptDetailsTips = contentPane.GetChildByPath("EquipPanel.EquiptDetailsTips") as GComponent;
                }
                return m_EquipPanel_EquiptDetailsTips;
            }
        }
        private GImage m_EquipPanel_EquiptDetailsTips_TipsBg;
        private GImage EquipPanel_EquiptDetailsTips_TipsBg
        {
            get {
                if (m_EquipPanel_EquiptDetailsTips_TipsBg == null)
                {
                    m_EquipPanel_EquiptDetailsTips_TipsBg = contentPane.GetChildByPath("EquipPanel.EquiptDetailsTips.TipsBg") as GImage;
                }
                return m_EquipPanel_EquiptDetailsTips_TipsBg;
            }
        }
        private GList m_EquipPanel_EquiptDetailsTips_DetailedList;
        private GList EquipPanel_EquiptDetailsTips_DetailedList
        {
            get {
                if (m_EquipPanel_EquiptDetailsTips_DetailedList == null)
                {
                    m_EquipPanel_EquiptDetailsTips_DetailedList = contentPane.GetChildByPath("EquipPanel.EquiptDetailsTips.DetailedList") as GList;
                }
                return m_EquipPanel_EquiptDetailsTips_DetailedList;
            }
        }
        private GComponent m_EquipPanel_EquiptDetailsTips_CommonEmptyState;
        private GComponent EquipPanel_EquiptDetailsTips_CommonEmptyState
        {
            get {
                if (m_EquipPanel_EquiptDetailsTips_CommonEmptyState == null)
                {
                    m_EquipPanel_EquiptDetailsTips_CommonEmptyState = contentPane.GetChildByPath("EquipPanel.EquiptDetailsTips.CommonEmptyState") as GComponent;
                }
                return m_EquipPanel_EquiptDetailsTips_CommonEmptyState;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel;
        private GComponent EquipPanel_FilterEquiptPanel
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel == null)
                {
                    m_EquipPanel_FilterEquiptPanel = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_Bg;
        private GImage EquipPanel_FilterEquiptPanel_Bg
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_Bg == null)
                {
                    m_EquipPanel_FilterEquiptPanel_Bg = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.Bg") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_Bg;
            }
        }
        private GTextField m_EquipPanel_FilterEquiptPanel_TitleTxt;
        private GTextField EquipPanel_FilterEquiptPanel_TitleTxt
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_TitleTxt == null)
                {
                    m_EquipPanel_FilterEquiptPanel_TitleTxt = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.TitleTxt") as GTextField;
                }
                return m_EquipPanel_FilterEquiptPanel_TitleTxt;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_StarImg;
        private GImage EquipPanel_FilterEquiptPanel_StarImg
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_StarImg == null)
                {
                    m_EquipPanel_FilterEquiptPanel_StarImg = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.StarImg") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_StarImg;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_StarImg16;
        private GImage EquipPanel_FilterEquiptPanel_StarImg16
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_StarImg16 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_StarImg16 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.StarImg") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_StarImg16;
            }
        }
        private GTextField m_EquipPanel_FilterEquiptPanel_TitleTxt17;
        private GTextField EquipPanel_FilterEquiptPanel_TitleTxt17
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_TitleTxt17 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_TitleTxt17 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.TitleTxt") as GTextField;
                }
                return m_EquipPanel_FilterEquiptPanel_TitleTxt17;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_StarImg18;
        private GImage EquipPanel_FilterEquiptPanel_StarImg18
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_StarImg18 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_StarImg18 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.StarImg") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_StarImg18;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_StarImg19;
        private GImage EquipPanel_FilterEquiptPanel_StarImg19
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_StarImg19 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_StarImg19 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.StarImg") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_StarImg19;
            }
        }
        private GButton m_EquipPanel_FilterEquiptPanel_ConfirmBtn;
        private GButton EquipPanel_FilterEquiptPanel_ConfirmBtn
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_ConfirmBtn == null)
                {
                    m_EquipPanel_FilterEquiptPanel_ConfirmBtn = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.ConfirmBtn") as GButton;
                }
                return m_EquipPanel_FilterEquiptPanel_ConfirmBtn;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n0;
        private GImage EquipPanel_FilterEquiptPanel_ConfirmBtn_n0
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n0 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n0 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.ConfirmBtn.n0") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n0;
            }
        }
        private GTextField m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n2;
        private GTextField EquipPanel_FilterEquiptPanel_ConfirmBtn_n2
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n2 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n2 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.ConfirmBtn.n2") as GTextField;
                }
                return m_EquipPanel_FilterEquiptPanel_ConfirmBtn_n2;
            }
        }
        private GButton m_EquipPanel_FilterEquiptPanel_PropertyBtn;
        private GButton EquipPanel_FilterEquiptPanel_PropertyBtn
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_PropertyBtn == null)
                {
                    m_EquipPanel_FilterEquiptPanel_PropertyBtn = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.PropertyBtn") as GButton;
                }
                return m_EquipPanel_FilterEquiptPanel_PropertyBtn;
            }
        }
        private GImage m_EquipPanel_FilterEquiptPanel_PropertyBtn_n0;
        private GImage EquipPanel_FilterEquiptPanel_PropertyBtn_n0
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_PropertyBtn_n0 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_PropertyBtn_n0 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.PropertyBtn.n0") as GImage;
                }
                return m_EquipPanel_FilterEquiptPanel_PropertyBtn_n0;
            }
        }
        private GTextField m_EquipPanel_FilterEquiptPanel_PropertyBtn_n2;
        private GTextField EquipPanel_FilterEquiptPanel_PropertyBtn_n2
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_PropertyBtn_n2 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_PropertyBtn_n2 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.PropertyBtn.n2") as GTextField;
                }
                return m_EquipPanel_FilterEquiptPanel_PropertyBtn_n2;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn01;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn01
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn01 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn01 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn01") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn01;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn02;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn02
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn02 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn02 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn02") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn02;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn03;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn03
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn03 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn03 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn03") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn03;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn04;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn04
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn04 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn04 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn04") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn04;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn05;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn05
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn05 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn05 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn05") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn05;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn06;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn06
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn06 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn06 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn06") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn06;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn07;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn07
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn07 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn07 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn07") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn07;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn08;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn08
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn08 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn08 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn08") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn08;
            }
        }
        private GComponent m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn09;
        private GComponent EquipPanel_FilterEquiptPanel_FiltrateStageBtn09
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn09 == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn09 = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateStageBtn09") as GComponent;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateStageBtn09;
            }
        }
        private GList m_EquipPanel_FilterEquiptPanel_FiltrateSuitGrp;
        private GList EquipPanel_FilterEquiptPanel_FiltrateSuitGrp
        {
            get {
                if (m_EquipPanel_FilterEquiptPanel_FiltrateSuitGrp == null)
                {
                    m_EquipPanel_FilterEquiptPanel_FiltrateSuitGrp = contentPane.GetChildByPath("EquipPanel.FilterEquiptPanel.FiltrateSuitGrp") as GList;
                }
                return m_EquipPanel_FilterEquiptPanel_FiltrateSuitGrp;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup;
        private GComponent EquipPanel_EquiptTipsGroup
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup == null)
                {
                    m_EquipPanel_EquiptTipsGroup = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips02;
        private GComponent EquipPanel_EquiptTipsGroup_Tips02
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02 == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02 = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02;
            }
        }
        private GImage m_EquipPanel_EquiptTipsGroup_Tips02_Bg;
        private GImage EquipPanel_EquiptTipsGroup_Tips02_Bg
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_Bg == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_Bg = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.Bg") as GImage;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_Bg;
            }
        }
        private GImage m_EquipPanel_EquiptTipsGroup_Tips02_Bg20;
        private GImage EquipPanel_EquiptTipsGroup_Tips02_Bg20
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_Bg20 == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_Bg20 = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.Bg") as GImage;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_Bg20;
            }
        }
        private GTextField m_EquipPanel_EquiptTipsGroup_Tips02_StageText;
        private GTextField EquipPanel_EquiptTipsGroup_Tips02_StageText
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_StageText == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_StageText = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.StageText") as GTextField;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_StageText;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips02_ButtonLock;
        private GComponent EquipPanel_EquiptTipsGroup_Tips02_ButtonLock
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_ButtonLock == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_ButtonLock = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.ButtonLock") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_ButtonLock;
            }
        }
        private GTextField m_EquipPanel_EquiptTipsGroup_Tips02_EquiptName;
        private GTextField EquipPanel_EquiptTipsGroup_Tips02_EquiptName
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_EquiptName == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_EquiptName = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.EquiptName") as GTextField;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_EquiptName;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips02_ItemFrame;
        private GComponent EquipPanel_EquiptTipsGroup_Tips02_ItemFrame
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_ItemFrame == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_ItemFrame = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.ItemFrame") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_ItemFrame;
            }
        }
        private GList m_EquipPanel_EquiptTipsGroup_Tips02_Content;
        private GList EquipPanel_EquiptTipsGroup_Tips02_Content
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_Content == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_Content = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.Content") as GList;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_Content;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips02_DressBtn;
        private GComponent EquipPanel_EquiptTipsGroup_Tips02_DressBtn
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_DressBtn == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_DressBtn = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.DressBtn") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_DressBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips02_StrengthenBtn;
        private GComponent EquipPanel_EquiptTipsGroup_Tips02_StrengthenBtn
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips02_StrengthenBtn == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips02_StrengthenBtn = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips02.StrengthenBtn") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips02_StrengthenBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips01;
        private GComponent EquipPanel_EquiptTipsGroup_Tips01
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01 == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01 = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01;
            }
        }
        private GImage m_EquipPanel_EquiptTipsGroup_Tips01_Bg;
        private GImage EquipPanel_EquiptTipsGroup_Tips01_Bg
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_Bg == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_Bg = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.Bg") as GImage;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_Bg;
            }
        }
        private GImage m_EquipPanel_EquiptTipsGroup_Tips01_Bg21;
        private GImage EquipPanel_EquiptTipsGroup_Tips01_Bg21
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_Bg21 == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_Bg21 = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.Bg") as GImage;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_Bg21;
            }
        }
        private GTextField m_EquipPanel_EquiptTipsGroup_Tips01_StageText;
        private GTextField EquipPanel_EquiptTipsGroup_Tips01_StageText
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_StageText == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_StageText = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.StageText") as GTextField;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_StageText;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips01_ButtonLock;
        private GComponent EquipPanel_EquiptTipsGroup_Tips01_ButtonLock
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_ButtonLock == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_ButtonLock = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.ButtonLock") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_ButtonLock;
            }
        }
        private GTextField m_EquipPanel_EquiptTipsGroup_Tips01_EquiptName;
        private GTextField EquipPanel_EquiptTipsGroup_Tips01_EquiptName
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_EquiptName == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_EquiptName = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.EquiptName") as GTextField;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_EquiptName;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips01_ItemFrame;
        private GComponent EquipPanel_EquiptTipsGroup_Tips01_ItemFrame
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_ItemFrame == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_ItemFrame = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.ItemFrame") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_ItemFrame;
            }
        }
        private GList m_EquipPanel_EquiptTipsGroup_Tips01_Content;
        private GList EquipPanel_EquiptTipsGroup_Tips01_Content
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_Content == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_Content = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.Content") as GList;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_Content;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips01_DressBtn;
        private GComponent EquipPanel_EquiptTipsGroup_Tips01_DressBtn
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_DressBtn == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_DressBtn = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.DressBtn") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_DressBtn;
            }
        }
        private GComponent m_EquipPanel_EquiptTipsGroup_Tips01_StrengthenBtn;
        private GComponent EquipPanel_EquiptTipsGroup_Tips01_StrengthenBtn
        {
            get {
                if (m_EquipPanel_EquiptTipsGroup_Tips01_StrengthenBtn == null)
                {
                    m_EquipPanel_EquiptTipsGroup_Tips01_StrengthenBtn = contentPane.GetChildByPath("EquipPanel.EquiptTipsGroup.Tips01.StrengthenBtn") as GComponent;
                }
                return m_EquipPanel_EquiptTipsGroup_Tips01_StrengthenBtn;
            }
        }
        private GComponent m_CardFilterTips;
        private GComponent CardFilterTips
        {
            get {
                if (m_CardFilterTips == null)
                {
                    m_CardFilterTips = contentPane.GetChildByPath("CardFilterTips") as GComponent;
                }
                return m_CardFilterTips;
            }
        }
        private GComponent m_TopInfo;
        private GComponent TopInfo
        {
            get {
                if (m_TopInfo == null)
                {
                    m_TopInfo = contentPane.GetChildByPath("TopInfo") as GComponent;
                }
                return m_TopInfo;
            }
        }
        private GRichTextField m_TopInfo_NameText;
        private GRichTextField TopInfo_NameText
        {
            get {
                if (m_TopInfo_NameText == null)
                {
                    m_TopInfo_NameText = contentPane.GetChildByPath("TopInfo.NameText") as GRichTextField;
                }
                return m_TopInfo_NameText;
            }
        }
        private GLoader m_TopInfo_StageIcon;
        private GLoader TopInfo_StageIcon
        {
            get {
                if (m_TopInfo_StageIcon == null)
                {
                    m_TopInfo_StageIcon = contentPane.GetChildByPath("TopInfo.StageIcon") as GLoader;
                }
                return m_TopInfo_StageIcon;
            }
        }
        private GTextField m_TopInfo_StageNumberTxt;
        private GTextField TopInfo_StageNumberTxt
        {
            get {
                if (m_TopInfo_StageNumberTxt == null)
                {
                    m_TopInfo_StageNumberTxt = contentPane.GetChildByPath("TopInfo.StageNumberTxt") as GTextField;
                }
                return m_TopInfo_StageNumberTxt;
            }
        }
        private GTextField m_TopInfo_LevelText;
        private GTextField TopInfo_LevelText
        {
            get {
                if (m_TopInfo_LevelText == null)
                {
                    m_TopInfo_LevelText = contentPane.GetChildByPath("TopInfo.LevelText") as GTextField;
                }
                return m_TopInfo_LevelText;
            }
        }
        private GList m_TopInfo_StarGroup01;
        private GList TopInfo_StarGroup01
        {
            get {
                if (m_TopInfo_StarGroup01 == null)
                {
                    m_TopInfo_StarGroup01 = contentPane.GetChildByPath("TopInfo.StarGroup01") as GList;
                }
                return m_TopInfo_StarGroup01;
            }
        }
        private GButton m_TopInfo_BtnNature;
        private GButton TopInfo_BtnNature
        {
            get {
                if (m_TopInfo_BtnNature == null)
                {
                    m_TopInfo_BtnNature = contentPane.GetChildByPath("TopInfo.BtnNature") as GButton;
                }
                return m_TopInfo_BtnNature;
            }
        }
        private GLoader m_TopInfo_BtnNature_NatureIcon;
        private GLoader TopInfo_BtnNature_NatureIcon
        {
            get {
                if (m_TopInfo_BtnNature_NatureIcon == null)
                {
                    m_TopInfo_BtnNature_NatureIcon = contentPane.GetChildByPath("TopInfo.BtnNature.NatureIcon") as GLoader;
                }
                return m_TopInfo_BtnNature_NatureIcon;
            }
        }
        private GImage m_TopInfo_BtnNature_ProfessionBg;
        private GImage TopInfo_BtnNature_ProfessionBg
        {
            get {
                if (m_TopInfo_BtnNature_ProfessionBg == null)
                {
                    m_TopInfo_BtnNature_ProfessionBg = contentPane.GetChildByPath("TopInfo.BtnNature.ProfessionBg") as GImage;
                }
                return m_TopInfo_BtnNature_ProfessionBg;
            }
        }
        private GLoader m_TopInfo_BtnNature_ProfessionIcon;
        private GLoader TopInfo_BtnNature_ProfessionIcon
        {
            get {
                if (m_TopInfo_BtnNature_ProfessionIcon == null)
                {
                    m_TopInfo_BtnNature_ProfessionIcon = contentPane.GetChildByPath("TopInfo.BtnNature.ProfessionIcon") as GLoader;
                }
                return m_TopInfo_BtnNature_ProfessionIcon;
            }
        }
        private GComponent m_AssetsTop;
        private GComponent AssetsTop
        {
            get {
                if (m_AssetsTop == null)
                {
                    m_AssetsTop = contentPane.GetChildByPath("AssetsTop") as GComponent;
                }
                return m_AssetsTop;
            }
        }
        private GComponent m_PropertyDetailsTips;
        private GComponent PropertyDetailsTips
        {
            get {
                if (m_PropertyDetailsTips == null)
                {
                    m_PropertyDetailsTips = contentPane.GetChildByPath("PropertyDetailsTips") as GComponent;
                }
                return m_PropertyDetailsTips;
            }
        }
    }
}
