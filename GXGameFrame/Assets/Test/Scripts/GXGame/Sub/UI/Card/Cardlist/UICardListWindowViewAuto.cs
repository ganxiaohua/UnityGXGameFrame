using FairyGUI;
using GameFrame;
namespace GXGame
{
    public partial class UICardListWindowView : UIViewBase
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
        private GImage m_Line;
        private GImage Line
        {
            get {
                if (m_Line == null)
                {
                    m_Line = contentPane.GetChildByPath("Line") as GImage;
                }
                return m_Line;
            }
        }
        private GButton m_FormBtn;
        private GButton FormBtn
        {
            get {
                if (m_FormBtn == null)
                {
                    m_FormBtn = contentPane.GetChildByPath("FormBtn") as GButton;
                }
                return m_FormBtn;
            }
        }
        private GImage m_FormBtn_n1;
        private GImage FormBtn_n1
        {
            get {
                if (m_FormBtn_n1 == null)
                {
                    m_FormBtn_n1 = contentPane.GetChildByPath("FormBtn.n1") as GImage;
                }
                return m_FormBtn_n1;
            }
        }
        private GImage m_FormBtn_n0;
        private GImage FormBtn_n0
        {
            get {
                if (m_FormBtn_n0 == null)
                {
                    m_FormBtn_n0 = contentPane.GetChildByPath("FormBtn.n0") as GImage;
                }
                return m_FormBtn_n0;
            }
        }
        private GImage m_n2;
        private GImage n2
        {
            get {
                if (m_n2 == null)
                {
                    m_n2 = contentPane.GetChildByPath("n2") as GImage;
                }
                return m_n2;
            }
        }
        private GImage m_n35;
        private GImage n35
        {
            get {
                if (m_n35 == null)
                {
                    m_n35 = contentPane.GetChildByPath("n35") as GImage;
                }
                return m_n35;
            }
        }
        private GTextField m_LvTxt;
        private GTextField LvTxt
        {
            get {
                if (m_LvTxt == null)
                {
                    m_LvTxt = contentPane.GetChildByPath("LvTxt") as GTextField;
                }
                return m_LvTxt;
            }
        }
        private GTextField m_n37;
        private GTextField n37
        {
            get {
                if (m_n37 == null)
                {
                    m_n37 = contentPane.GetChildByPath("n37") as GTextField;
                }
                return m_n37;
            }
        }
        private GImage m_Line01;
        private GImage Line01
        {
            get {
                if (m_Line01 == null)
                {
                    m_Line01 = contentPane.GetChildByPath("Line01") as GImage;
                }
                return m_Line01;
            }
        }
        private GImage m_Line02;
        private GImage Line02
        {
            get {
                if (m_Line02 == null)
                {
                    m_Line02 = contentPane.GetChildByPath("Line02") as GImage;
                }
                return m_Line02;
            }
        }
        private GImage m_Line03;
        private GImage Line03
        {
            get {
                if (m_Line03 == null)
                {
                    m_Line03 = contentPane.GetChildByPath("Line03") as GImage;
                }
                return m_Line03;
            }
        }
        private GImage m_Line04;
        private GImage Line04
        {
            get {
                if (m_Line04 == null)
                {
                    m_Line04 = contentPane.GetChildByPath("Line04") as GImage;
                }
                return m_Line04;
            }
        }
        private GImage m_Line05;
        private GImage Line05
        {
            get {
                if (m_Line05 == null)
                {
                    m_Line05 = contentPane.GetChildByPath("Line05") as GImage;
                }
                return m_Line05;
            }
        }
        private GComponent m_CardShareinfoItem01;
        private GComponent CardShareinfoItem01
        {
            get {
                if (m_CardShareinfoItem01 == null)
                {
                    m_CardShareinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem01;
            }
        }
        private GButton m_CardShareinfoItem01_AddBtn;
        private GButton CardShareinfoItem01_AddBtn
        {
            get {
                if (m_CardShareinfoItem01_AddBtn == null)
                {
                    m_CardShareinfoItem01_AddBtn = contentPane.GetChildByPath("CardShareinfoItem01.AddBtn") as GButton;
                }
                return m_CardShareinfoItem01_AddBtn;
            }
        }
        private GImage m_CardShareinfoItem01_AddBtn_n0;
        private GImage CardShareinfoItem01_AddBtn_n0
        {
            get {
                if (m_CardShareinfoItem01_AddBtn_n0 == null)
                {
                    m_CardShareinfoItem01_AddBtn_n0 = contentPane.GetChildByPath("CardShareinfoItem01.AddBtn.n0") as GImage;
                }
                return m_CardShareinfoItem01_AddBtn_n0;
            }
        }
        private GImage m_CardShareinfoItem01_AddBtn_n1;
        private GImage CardShareinfoItem01_AddBtn_n1
        {
            get {
                if (m_CardShareinfoItem01_AddBtn_n1 == null)
                {
                    m_CardShareinfoItem01_AddBtn_n1 = contentPane.GetChildByPath("CardShareinfoItem01.AddBtn.n1") as GImage;
                }
                return m_CardShareinfoItem01_AddBtn_n1;
            }
        }
        private GTextField m_CardShareinfoItem01_AddBtn_txt;
        private GTextField CardShareinfoItem01_AddBtn_txt
        {
            get {
                if (m_CardShareinfoItem01_AddBtn_txt == null)
                {
                    m_CardShareinfoItem01_AddBtn_txt = contentPane.GetChildByPath("CardShareinfoItem01.AddBtn.txt") as GTextField;
                }
                return m_CardShareinfoItem01_AddBtn_txt;
            }
        }
        private GComponent m_CardShareinfoItem01_CardinfoItem01;
        private GComponent CardShareinfoItem01_CardinfoItem01
        {
            get {
                if (m_CardShareinfoItem01_CardinfoItem01 == null)
                {
                    m_CardShareinfoItem01_CardinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem01.CardinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem01_CardinfoItem01;
            }
        }
        private GImage m_CardShareinfoItem01_ActivateBg;
        private GImage CardShareinfoItem01_ActivateBg
        {
            get {
                if (m_CardShareinfoItem01_ActivateBg == null)
                {
                    m_CardShareinfoItem01_ActivateBg = contentPane.GetChildByPath("CardShareinfoItem01.ActivateBg") as GImage;
                }
                return m_CardShareinfoItem01_ActivateBg;
            }
        }
        private GImage m_CardShareinfoItem01_Mask;
        private GImage CardShareinfoItem01_Mask
        {
            get {
                if (m_CardShareinfoItem01_Mask == null)
                {
                    m_CardShareinfoItem01_Mask = contentPane.GetChildByPath("CardShareinfoItem01.Mask") as GImage;
                }
                return m_CardShareinfoItem01_Mask;
            }
        }
        private GButton m_CardShareinfoItem01_BtnChange;
        private GButton CardShareinfoItem01_BtnChange
        {
            get {
                if (m_CardShareinfoItem01_BtnChange == null)
                {
                    m_CardShareinfoItem01_BtnChange = contentPane.GetChildByPath("CardShareinfoItem01.BtnChange") as GButton;
                }
                return m_CardShareinfoItem01_BtnChange;
            }
        }
        private GImage m_CardShareinfoItem01_BtnChange_Bg;
        private GImage CardShareinfoItem01_BtnChange_Bg
        {
            get {
                if (m_CardShareinfoItem01_BtnChange_Bg == null)
                {
                    m_CardShareinfoItem01_BtnChange_Bg = contentPane.GetChildByPath("CardShareinfoItem01.BtnChange.Bg") as GImage;
                }
                return m_CardShareinfoItem01_BtnChange_Bg;
            }
        }
        private GImage m_CardShareinfoItem01_BtnChange_n1;
        private GImage CardShareinfoItem01_BtnChange_n1
        {
            get {
                if (m_CardShareinfoItem01_BtnChange_n1 == null)
                {
                    m_CardShareinfoItem01_BtnChange_n1 = contentPane.GetChildByPath("CardShareinfoItem01.BtnChange.n1") as GImage;
                }
                return m_CardShareinfoItem01_BtnChange_n1;
            }
        }
        private GImage m_CardShareinfoItem01_BtnMask;
        private GImage CardShareinfoItem01_BtnMask
        {
            get {
                if (m_CardShareinfoItem01_BtnMask == null)
                {
                    m_CardShareinfoItem01_BtnMask = contentPane.GetChildByPath("CardShareinfoItem01.BtnMask") as GImage;
                }
                return m_CardShareinfoItem01_BtnMask;
            }
        }
        private GComponent m_CardShareinfoItem02;
        private GComponent CardShareinfoItem02
        {
            get {
                if (m_CardShareinfoItem02 == null)
                {
                    m_CardShareinfoItem02 = contentPane.GetChildByPath("CardShareinfoItem02") as GComponent;
                }
                return m_CardShareinfoItem02;
            }
        }
        private GButton m_CardShareinfoItem02_AddBtn;
        private GButton CardShareinfoItem02_AddBtn
        {
            get {
                if (m_CardShareinfoItem02_AddBtn == null)
                {
                    m_CardShareinfoItem02_AddBtn = contentPane.GetChildByPath("CardShareinfoItem02.AddBtn") as GButton;
                }
                return m_CardShareinfoItem02_AddBtn;
            }
        }
        private GImage m_CardShareinfoItem02_AddBtn_n0;
        private GImage CardShareinfoItem02_AddBtn_n0
        {
            get {
                if (m_CardShareinfoItem02_AddBtn_n0 == null)
                {
                    m_CardShareinfoItem02_AddBtn_n0 = contentPane.GetChildByPath("CardShareinfoItem02.AddBtn.n0") as GImage;
                }
                return m_CardShareinfoItem02_AddBtn_n0;
            }
        }
        private GImage m_CardShareinfoItem02_AddBtn_n1;
        private GImage CardShareinfoItem02_AddBtn_n1
        {
            get {
                if (m_CardShareinfoItem02_AddBtn_n1 == null)
                {
                    m_CardShareinfoItem02_AddBtn_n1 = contentPane.GetChildByPath("CardShareinfoItem02.AddBtn.n1") as GImage;
                }
                return m_CardShareinfoItem02_AddBtn_n1;
            }
        }
        private GTextField m_CardShareinfoItem02_AddBtn_txt;
        private GTextField CardShareinfoItem02_AddBtn_txt
        {
            get {
                if (m_CardShareinfoItem02_AddBtn_txt == null)
                {
                    m_CardShareinfoItem02_AddBtn_txt = contentPane.GetChildByPath("CardShareinfoItem02.AddBtn.txt") as GTextField;
                }
                return m_CardShareinfoItem02_AddBtn_txt;
            }
        }
        private GComponent m_CardShareinfoItem02_CardinfoItem01;
        private GComponent CardShareinfoItem02_CardinfoItem01
        {
            get {
                if (m_CardShareinfoItem02_CardinfoItem01 == null)
                {
                    m_CardShareinfoItem02_CardinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem02.CardinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem02_CardinfoItem01;
            }
        }
        private GImage m_CardShareinfoItem02_ActivateBg;
        private GImage CardShareinfoItem02_ActivateBg
        {
            get {
                if (m_CardShareinfoItem02_ActivateBg == null)
                {
                    m_CardShareinfoItem02_ActivateBg = contentPane.GetChildByPath("CardShareinfoItem02.ActivateBg") as GImage;
                }
                return m_CardShareinfoItem02_ActivateBg;
            }
        }
        private GImage m_CardShareinfoItem02_Mask;
        private GImage CardShareinfoItem02_Mask
        {
            get {
                if (m_CardShareinfoItem02_Mask == null)
                {
                    m_CardShareinfoItem02_Mask = contentPane.GetChildByPath("CardShareinfoItem02.Mask") as GImage;
                }
                return m_CardShareinfoItem02_Mask;
            }
        }
        private GButton m_CardShareinfoItem02_BtnChange;
        private GButton CardShareinfoItem02_BtnChange
        {
            get {
                if (m_CardShareinfoItem02_BtnChange == null)
                {
                    m_CardShareinfoItem02_BtnChange = contentPane.GetChildByPath("CardShareinfoItem02.BtnChange") as GButton;
                }
                return m_CardShareinfoItem02_BtnChange;
            }
        }
        private GImage m_CardShareinfoItem02_BtnChange_Bg;
        private GImage CardShareinfoItem02_BtnChange_Bg
        {
            get {
                if (m_CardShareinfoItem02_BtnChange_Bg == null)
                {
                    m_CardShareinfoItem02_BtnChange_Bg = contentPane.GetChildByPath("CardShareinfoItem02.BtnChange.Bg") as GImage;
                }
                return m_CardShareinfoItem02_BtnChange_Bg;
            }
        }
        private GImage m_CardShareinfoItem02_BtnChange_n1;
        private GImage CardShareinfoItem02_BtnChange_n1
        {
            get {
                if (m_CardShareinfoItem02_BtnChange_n1 == null)
                {
                    m_CardShareinfoItem02_BtnChange_n1 = contentPane.GetChildByPath("CardShareinfoItem02.BtnChange.n1") as GImage;
                }
                return m_CardShareinfoItem02_BtnChange_n1;
            }
        }
        private GImage m_CardShareinfoItem02_BtnMask;
        private GImage CardShareinfoItem02_BtnMask
        {
            get {
                if (m_CardShareinfoItem02_BtnMask == null)
                {
                    m_CardShareinfoItem02_BtnMask = contentPane.GetChildByPath("CardShareinfoItem02.BtnMask") as GImage;
                }
                return m_CardShareinfoItem02_BtnMask;
            }
        }
        private GComponent m_CardShareinfoItem03;
        private GComponent CardShareinfoItem03
        {
            get {
                if (m_CardShareinfoItem03 == null)
                {
                    m_CardShareinfoItem03 = contentPane.GetChildByPath("CardShareinfoItem03") as GComponent;
                }
                return m_CardShareinfoItem03;
            }
        }
        private GButton m_CardShareinfoItem03_AddBtn;
        private GButton CardShareinfoItem03_AddBtn
        {
            get {
                if (m_CardShareinfoItem03_AddBtn == null)
                {
                    m_CardShareinfoItem03_AddBtn = contentPane.GetChildByPath("CardShareinfoItem03.AddBtn") as GButton;
                }
                return m_CardShareinfoItem03_AddBtn;
            }
        }
        private GImage m_CardShareinfoItem03_AddBtn_n0;
        private GImage CardShareinfoItem03_AddBtn_n0
        {
            get {
                if (m_CardShareinfoItem03_AddBtn_n0 == null)
                {
                    m_CardShareinfoItem03_AddBtn_n0 = contentPane.GetChildByPath("CardShareinfoItem03.AddBtn.n0") as GImage;
                }
                return m_CardShareinfoItem03_AddBtn_n0;
            }
        }
        private GImage m_CardShareinfoItem03_AddBtn_n1;
        private GImage CardShareinfoItem03_AddBtn_n1
        {
            get {
                if (m_CardShareinfoItem03_AddBtn_n1 == null)
                {
                    m_CardShareinfoItem03_AddBtn_n1 = contentPane.GetChildByPath("CardShareinfoItem03.AddBtn.n1") as GImage;
                }
                return m_CardShareinfoItem03_AddBtn_n1;
            }
        }
        private GTextField m_CardShareinfoItem03_AddBtn_txt;
        private GTextField CardShareinfoItem03_AddBtn_txt
        {
            get {
                if (m_CardShareinfoItem03_AddBtn_txt == null)
                {
                    m_CardShareinfoItem03_AddBtn_txt = contentPane.GetChildByPath("CardShareinfoItem03.AddBtn.txt") as GTextField;
                }
                return m_CardShareinfoItem03_AddBtn_txt;
            }
        }
        private GComponent m_CardShareinfoItem03_CardinfoItem01;
        private GComponent CardShareinfoItem03_CardinfoItem01
        {
            get {
                if (m_CardShareinfoItem03_CardinfoItem01 == null)
                {
                    m_CardShareinfoItem03_CardinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem03.CardinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem03_CardinfoItem01;
            }
        }
        private GImage m_CardShareinfoItem03_ActivateBg;
        private GImage CardShareinfoItem03_ActivateBg
        {
            get {
                if (m_CardShareinfoItem03_ActivateBg == null)
                {
                    m_CardShareinfoItem03_ActivateBg = contentPane.GetChildByPath("CardShareinfoItem03.ActivateBg") as GImage;
                }
                return m_CardShareinfoItem03_ActivateBg;
            }
        }
        private GImage m_CardShareinfoItem03_Mask;
        private GImage CardShareinfoItem03_Mask
        {
            get {
                if (m_CardShareinfoItem03_Mask == null)
                {
                    m_CardShareinfoItem03_Mask = contentPane.GetChildByPath("CardShareinfoItem03.Mask") as GImage;
                }
                return m_CardShareinfoItem03_Mask;
            }
        }
        private GButton m_CardShareinfoItem03_BtnChange;
        private GButton CardShareinfoItem03_BtnChange
        {
            get {
                if (m_CardShareinfoItem03_BtnChange == null)
                {
                    m_CardShareinfoItem03_BtnChange = contentPane.GetChildByPath("CardShareinfoItem03.BtnChange") as GButton;
                }
                return m_CardShareinfoItem03_BtnChange;
            }
        }
        private GImage m_CardShareinfoItem03_BtnChange_Bg;
        private GImage CardShareinfoItem03_BtnChange_Bg
        {
            get {
                if (m_CardShareinfoItem03_BtnChange_Bg == null)
                {
                    m_CardShareinfoItem03_BtnChange_Bg = contentPane.GetChildByPath("CardShareinfoItem03.BtnChange.Bg") as GImage;
                }
                return m_CardShareinfoItem03_BtnChange_Bg;
            }
        }
        private GImage m_CardShareinfoItem03_BtnChange_n1;
        private GImage CardShareinfoItem03_BtnChange_n1
        {
            get {
                if (m_CardShareinfoItem03_BtnChange_n1 == null)
                {
                    m_CardShareinfoItem03_BtnChange_n1 = contentPane.GetChildByPath("CardShareinfoItem03.BtnChange.n1") as GImage;
                }
                return m_CardShareinfoItem03_BtnChange_n1;
            }
        }
        private GImage m_CardShareinfoItem03_BtnMask;
        private GImage CardShareinfoItem03_BtnMask
        {
            get {
                if (m_CardShareinfoItem03_BtnMask == null)
                {
                    m_CardShareinfoItem03_BtnMask = contentPane.GetChildByPath("CardShareinfoItem03.BtnMask") as GImage;
                }
                return m_CardShareinfoItem03_BtnMask;
            }
        }
        private GComponent m_CardShareinfoItem04;
        private GComponent CardShareinfoItem04
        {
            get {
                if (m_CardShareinfoItem04 == null)
                {
                    m_CardShareinfoItem04 = contentPane.GetChildByPath("CardShareinfoItem04") as GComponent;
                }
                return m_CardShareinfoItem04;
            }
        }
        private GButton m_CardShareinfoItem04_AddBtn;
        private GButton CardShareinfoItem04_AddBtn
        {
            get {
                if (m_CardShareinfoItem04_AddBtn == null)
                {
                    m_CardShareinfoItem04_AddBtn = contentPane.GetChildByPath("CardShareinfoItem04.AddBtn") as GButton;
                }
                return m_CardShareinfoItem04_AddBtn;
            }
        }
        private GImage m_CardShareinfoItem04_AddBtn_n0;
        private GImage CardShareinfoItem04_AddBtn_n0
        {
            get {
                if (m_CardShareinfoItem04_AddBtn_n0 == null)
                {
                    m_CardShareinfoItem04_AddBtn_n0 = contentPane.GetChildByPath("CardShareinfoItem04.AddBtn.n0") as GImage;
                }
                return m_CardShareinfoItem04_AddBtn_n0;
            }
        }
        private GImage m_CardShareinfoItem04_AddBtn_n1;
        private GImage CardShareinfoItem04_AddBtn_n1
        {
            get {
                if (m_CardShareinfoItem04_AddBtn_n1 == null)
                {
                    m_CardShareinfoItem04_AddBtn_n1 = contentPane.GetChildByPath("CardShareinfoItem04.AddBtn.n1") as GImage;
                }
                return m_CardShareinfoItem04_AddBtn_n1;
            }
        }
        private GTextField m_CardShareinfoItem04_AddBtn_txt;
        private GTextField CardShareinfoItem04_AddBtn_txt
        {
            get {
                if (m_CardShareinfoItem04_AddBtn_txt == null)
                {
                    m_CardShareinfoItem04_AddBtn_txt = contentPane.GetChildByPath("CardShareinfoItem04.AddBtn.txt") as GTextField;
                }
                return m_CardShareinfoItem04_AddBtn_txt;
            }
        }
        private GComponent m_CardShareinfoItem04_CardinfoItem01;
        private GComponent CardShareinfoItem04_CardinfoItem01
        {
            get {
                if (m_CardShareinfoItem04_CardinfoItem01 == null)
                {
                    m_CardShareinfoItem04_CardinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem04.CardinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem04_CardinfoItem01;
            }
        }
        private GImage m_CardShareinfoItem04_ActivateBg;
        private GImage CardShareinfoItem04_ActivateBg
        {
            get {
                if (m_CardShareinfoItem04_ActivateBg == null)
                {
                    m_CardShareinfoItem04_ActivateBg = contentPane.GetChildByPath("CardShareinfoItem04.ActivateBg") as GImage;
                }
                return m_CardShareinfoItem04_ActivateBg;
            }
        }
        private GImage m_CardShareinfoItem04_Mask;
        private GImage CardShareinfoItem04_Mask
        {
            get {
                if (m_CardShareinfoItem04_Mask == null)
                {
                    m_CardShareinfoItem04_Mask = contentPane.GetChildByPath("CardShareinfoItem04.Mask") as GImage;
                }
                return m_CardShareinfoItem04_Mask;
            }
        }
        private GButton m_CardShareinfoItem04_BtnChange;
        private GButton CardShareinfoItem04_BtnChange
        {
            get {
                if (m_CardShareinfoItem04_BtnChange == null)
                {
                    m_CardShareinfoItem04_BtnChange = contentPane.GetChildByPath("CardShareinfoItem04.BtnChange") as GButton;
                }
                return m_CardShareinfoItem04_BtnChange;
            }
        }
        private GImage m_CardShareinfoItem04_BtnChange_Bg;
        private GImage CardShareinfoItem04_BtnChange_Bg
        {
            get {
                if (m_CardShareinfoItem04_BtnChange_Bg == null)
                {
                    m_CardShareinfoItem04_BtnChange_Bg = contentPane.GetChildByPath("CardShareinfoItem04.BtnChange.Bg") as GImage;
                }
                return m_CardShareinfoItem04_BtnChange_Bg;
            }
        }
        private GImage m_CardShareinfoItem04_BtnChange_n1;
        private GImage CardShareinfoItem04_BtnChange_n1
        {
            get {
                if (m_CardShareinfoItem04_BtnChange_n1 == null)
                {
                    m_CardShareinfoItem04_BtnChange_n1 = contentPane.GetChildByPath("CardShareinfoItem04.BtnChange.n1") as GImage;
                }
                return m_CardShareinfoItem04_BtnChange_n1;
            }
        }
        private GImage m_CardShareinfoItem04_BtnMask;
        private GImage CardShareinfoItem04_BtnMask
        {
            get {
                if (m_CardShareinfoItem04_BtnMask == null)
                {
                    m_CardShareinfoItem04_BtnMask = contentPane.GetChildByPath("CardShareinfoItem04.BtnMask") as GImage;
                }
                return m_CardShareinfoItem04_BtnMask;
            }
        }
        private GComponent m_CardShareinfoItem05;
        private GComponent CardShareinfoItem05
        {
            get {
                if (m_CardShareinfoItem05 == null)
                {
                    m_CardShareinfoItem05 = contentPane.GetChildByPath("CardShareinfoItem05") as GComponent;
                }
                return m_CardShareinfoItem05;
            }
        }
        private GButton m_CardShareinfoItem05_AddBtn;
        private GButton CardShareinfoItem05_AddBtn
        {
            get {
                if (m_CardShareinfoItem05_AddBtn == null)
                {
                    m_CardShareinfoItem05_AddBtn = contentPane.GetChildByPath("CardShareinfoItem05.AddBtn") as GButton;
                }
                return m_CardShareinfoItem05_AddBtn;
            }
        }
        private GImage m_CardShareinfoItem05_AddBtn_n0;
        private GImage CardShareinfoItem05_AddBtn_n0
        {
            get {
                if (m_CardShareinfoItem05_AddBtn_n0 == null)
                {
                    m_CardShareinfoItem05_AddBtn_n0 = contentPane.GetChildByPath("CardShareinfoItem05.AddBtn.n0") as GImage;
                }
                return m_CardShareinfoItem05_AddBtn_n0;
            }
        }
        private GImage m_CardShareinfoItem05_AddBtn_n1;
        private GImage CardShareinfoItem05_AddBtn_n1
        {
            get {
                if (m_CardShareinfoItem05_AddBtn_n1 == null)
                {
                    m_CardShareinfoItem05_AddBtn_n1 = contentPane.GetChildByPath("CardShareinfoItem05.AddBtn.n1") as GImage;
                }
                return m_CardShareinfoItem05_AddBtn_n1;
            }
        }
        private GTextField m_CardShareinfoItem05_AddBtn_txt;
        private GTextField CardShareinfoItem05_AddBtn_txt
        {
            get {
                if (m_CardShareinfoItem05_AddBtn_txt == null)
                {
                    m_CardShareinfoItem05_AddBtn_txt = contentPane.GetChildByPath("CardShareinfoItem05.AddBtn.txt") as GTextField;
                }
                return m_CardShareinfoItem05_AddBtn_txt;
            }
        }
        private GComponent m_CardShareinfoItem05_CardinfoItem01;
        private GComponent CardShareinfoItem05_CardinfoItem01
        {
            get {
                if (m_CardShareinfoItem05_CardinfoItem01 == null)
                {
                    m_CardShareinfoItem05_CardinfoItem01 = contentPane.GetChildByPath("CardShareinfoItem05.CardinfoItem01") as GComponent;
                }
                return m_CardShareinfoItem05_CardinfoItem01;
            }
        }
        private GImage m_CardShareinfoItem05_ActivateBg;
        private GImage CardShareinfoItem05_ActivateBg
        {
            get {
                if (m_CardShareinfoItem05_ActivateBg == null)
                {
                    m_CardShareinfoItem05_ActivateBg = contentPane.GetChildByPath("CardShareinfoItem05.ActivateBg") as GImage;
                }
                return m_CardShareinfoItem05_ActivateBg;
            }
        }
        private GImage m_CardShareinfoItem05_Mask;
        private GImage CardShareinfoItem05_Mask
        {
            get {
                if (m_CardShareinfoItem05_Mask == null)
                {
                    m_CardShareinfoItem05_Mask = contentPane.GetChildByPath("CardShareinfoItem05.Mask") as GImage;
                }
                return m_CardShareinfoItem05_Mask;
            }
        }
        private GButton m_CardShareinfoItem05_BtnChange;
        private GButton CardShareinfoItem05_BtnChange
        {
            get {
                if (m_CardShareinfoItem05_BtnChange == null)
                {
                    m_CardShareinfoItem05_BtnChange = contentPane.GetChildByPath("CardShareinfoItem05.BtnChange") as GButton;
                }
                return m_CardShareinfoItem05_BtnChange;
            }
        }
        private GImage m_CardShareinfoItem05_BtnChange_Bg;
        private GImage CardShareinfoItem05_BtnChange_Bg
        {
            get {
                if (m_CardShareinfoItem05_BtnChange_Bg == null)
                {
                    m_CardShareinfoItem05_BtnChange_Bg = contentPane.GetChildByPath("CardShareinfoItem05.BtnChange.Bg") as GImage;
                }
                return m_CardShareinfoItem05_BtnChange_Bg;
            }
        }
        private GImage m_CardShareinfoItem05_BtnChange_n1;
        private GImage CardShareinfoItem05_BtnChange_n1
        {
            get {
                if (m_CardShareinfoItem05_BtnChange_n1 == null)
                {
                    m_CardShareinfoItem05_BtnChange_n1 = contentPane.GetChildByPath("CardShareinfoItem05.BtnChange.n1") as GImage;
                }
                return m_CardShareinfoItem05_BtnChange_n1;
            }
        }
        private GImage m_CardShareinfoItem05_BtnMask;
        private GImage CardShareinfoItem05_BtnMask
        {
            get {
                if (m_CardShareinfoItem05_BtnMask == null)
                {
                    m_CardShareinfoItem05_BtnMask = contentPane.GetChildByPath("CardShareinfoItem05.BtnMask") as GImage;
                }
                return m_CardShareinfoItem05_BtnMask;
            }
        }
        private GComponent m_TextGroup;
        private GComponent TextGroup
        {
            get {
                if (m_TextGroup == null)
                {
                    m_TextGroup = contentPane.GetChildByPath("TextGroup") as GComponent;
                }
                return m_TextGroup;
            }
        }
        private GComponent m_ConfirmBtn;
        private GComponent ConfirmBtn
        {
            get {
                if (m_ConfirmBtn == null)
                {
                    m_ConfirmBtn = contentPane.GetChildByPath("ConfirmBtn") as GComponent;
                }
                return m_ConfirmBtn;
            }
        }
        private GImage m_FilterBg;
        private GImage FilterBg
        {
            get {
                if (m_FilterBg == null)
                {
                    m_FilterBg = contentPane.GetChildByPath("FilterBg") as GImage;
                }
                return m_FilterBg;
            }
        }
        private GButton m_EquiptFilterBtn01;
        private GButton EquiptFilterBtn01
        {
            get {
                if (m_EquiptFilterBtn01 == null)
                {
                    m_EquiptFilterBtn01 = contentPane.GetChildByPath("EquiptFilterBtn01") as GButton;
                }
                return m_EquiptFilterBtn01;
            }
        }
        private GImage m_EquiptFilterBtn01_Bg;
        private GImage EquiptFilterBtn01_Bg
        {
            get {
                if (m_EquiptFilterBtn01_Bg == null)
                {
                    m_EquiptFilterBtn01_Bg = contentPane.GetChildByPath("EquiptFilterBtn01.Bg") as GImage;
                }
                return m_EquiptFilterBtn01_Bg;
            }
        }
        private GLoader m_EquiptFilterBtn01_n8;
        private GLoader EquiptFilterBtn01_n8
        {
            get {
                if (m_EquiptFilterBtn01_n8 == null)
                {
                    m_EquiptFilterBtn01_n8 = contentPane.GetChildByPath("EquiptFilterBtn01.n8") as GLoader;
                }
                return m_EquiptFilterBtn01_n8;
            }
        }
        private GTextField m_EquiptFilterBtn01_Txt;
        private GTextField EquiptFilterBtn01_Txt
        {
            get {
                if (m_EquiptFilterBtn01_Txt == null)
                {
                    m_EquiptFilterBtn01_Txt = contentPane.GetChildByPath("EquiptFilterBtn01.Txt") as GTextField;
                }
                return m_EquiptFilterBtn01_Txt;
            }
        }
        private GButton m_EquiptFilterBtn02;
        private GButton EquiptFilterBtn02
        {
            get {
                if (m_EquiptFilterBtn02 == null)
                {
                    m_EquiptFilterBtn02 = contentPane.GetChildByPath("EquiptFilterBtn02") as GButton;
                }
                return m_EquiptFilterBtn02;
            }
        }
        private GImage m_EquiptFilterBtn02_Bg;
        private GImage EquiptFilterBtn02_Bg
        {
            get {
                if (m_EquiptFilterBtn02_Bg == null)
                {
                    m_EquiptFilterBtn02_Bg = contentPane.GetChildByPath("EquiptFilterBtn02.Bg") as GImage;
                }
                return m_EquiptFilterBtn02_Bg;
            }
        }
        private GLoader m_EquiptFilterBtn02_n8;
        private GLoader EquiptFilterBtn02_n8
        {
            get {
                if (m_EquiptFilterBtn02_n8 == null)
                {
                    m_EquiptFilterBtn02_n8 = contentPane.GetChildByPath("EquiptFilterBtn02.n8") as GLoader;
                }
                return m_EquiptFilterBtn02_n8;
            }
        }
        private GTextField m_EquiptFilterBtn02_Txt;
        private GTextField EquiptFilterBtn02_Txt
        {
            get {
                if (m_EquiptFilterBtn02_Txt == null)
                {
                    m_EquiptFilterBtn02_Txt = contentPane.GetChildByPath("EquiptFilterBtn02.Txt") as GTextField;
                }
                return m_EquiptFilterBtn02_Txt;
            }
        }
        private GButton m_EquiptFilterBtn03;
        private GButton EquiptFilterBtn03
        {
            get {
                if (m_EquiptFilterBtn03 == null)
                {
                    m_EquiptFilterBtn03 = contentPane.GetChildByPath("EquiptFilterBtn03") as GButton;
                }
                return m_EquiptFilterBtn03;
            }
        }
        private GImage m_EquiptFilterBtn03_Bg;
        private GImage EquiptFilterBtn03_Bg
        {
            get {
                if (m_EquiptFilterBtn03_Bg == null)
                {
                    m_EquiptFilterBtn03_Bg = contentPane.GetChildByPath("EquiptFilterBtn03.Bg") as GImage;
                }
                return m_EquiptFilterBtn03_Bg;
            }
        }
        private GLoader m_EquiptFilterBtn03_n8;
        private GLoader EquiptFilterBtn03_n8
        {
            get {
                if (m_EquiptFilterBtn03_n8 == null)
                {
                    m_EquiptFilterBtn03_n8 = contentPane.GetChildByPath("EquiptFilterBtn03.n8") as GLoader;
                }
                return m_EquiptFilterBtn03_n8;
            }
        }
        private GTextField m_EquiptFilterBtn03_Txt;
        private GTextField EquiptFilterBtn03_Txt
        {
            get {
                if (m_EquiptFilterBtn03_Txt == null)
                {
                    m_EquiptFilterBtn03_Txt = contentPane.GetChildByPath("EquiptFilterBtn03.Txt") as GTextField;
                }
                return m_EquiptFilterBtn03_Txt;
            }
        }
        private GButton m_EquiptFilterBtn04;
        private GButton EquiptFilterBtn04
        {
            get {
                if (m_EquiptFilterBtn04 == null)
                {
                    m_EquiptFilterBtn04 = contentPane.GetChildByPath("EquiptFilterBtn04") as GButton;
                }
                return m_EquiptFilterBtn04;
            }
        }
        private GImage m_EquiptFilterBtn04_Bg;
        private GImage EquiptFilterBtn04_Bg
        {
            get {
                if (m_EquiptFilterBtn04_Bg == null)
                {
                    m_EquiptFilterBtn04_Bg = contentPane.GetChildByPath("EquiptFilterBtn04.Bg") as GImage;
                }
                return m_EquiptFilterBtn04_Bg;
            }
        }
        private GLoader m_EquiptFilterBtn04_n8;
        private GLoader EquiptFilterBtn04_n8
        {
            get {
                if (m_EquiptFilterBtn04_n8 == null)
                {
                    m_EquiptFilterBtn04_n8 = contentPane.GetChildByPath("EquiptFilterBtn04.n8") as GLoader;
                }
                return m_EquiptFilterBtn04_n8;
            }
        }
        private GTextField m_EquiptFilterBtn04_Txt;
        private GTextField EquiptFilterBtn04_Txt
        {
            get {
                if (m_EquiptFilterBtn04_Txt == null)
                {
                    m_EquiptFilterBtn04_Txt = contentPane.GetChildByPath("EquiptFilterBtn04.Txt") as GTextField;
                }
                return m_EquiptFilterBtn04_Txt;
            }
        }
        private GButton m_EquiptFilterBtn05;
        private GButton EquiptFilterBtn05
        {
            get {
                if (m_EquiptFilterBtn05 == null)
                {
                    m_EquiptFilterBtn05 = contentPane.GetChildByPath("EquiptFilterBtn05") as GButton;
                }
                return m_EquiptFilterBtn05;
            }
        }
        private GImage m_EquiptFilterBtn05_Bg;
        private GImage EquiptFilterBtn05_Bg
        {
            get {
                if (m_EquiptFilterBtn05_Bg == null)
                {
                    m_EquiptFilterBtn05_Bg = contentPane.GetChildByPath("EquiptFilterBtn05.Bg") as GImage;
                }
                return m_EquiptFilterBtn05_Bg;
            }
        }
        private GLoader m_EquiptFilterBtn05_n8;
        private GLoader EquiptFilterBtn05_n8
        {
            get {
                if (m_EquiptFilterBtn05_n8 == null)
                {
                    m_EquiptFilterBtn05_n8 = contentPane.GetChildByPath("EquiptFilterBtn05.n8") as GLoader;
                }
                return m_EquiptFilterBtn05_n8;
            }
        }
        private GTextField m_EquiptFilterBtn05_Txt;
        private GTextField EquiptFilterBtn05_Txt
        {
            get {
                if (m_EquiptFilterBtn05_Txt == null)
                {
                    m_EquiptFilterBtn05_Txt = contentPane.GetChildByPath("EquiptFilterBtn05.Txt") as GTextField;
                }
                return m_EquiptFilterBtn05_Txt;
            }
        }
        private GList m_Cardinfolist;
        private GList Cardinfolist
        {
            get {
                if (m_Cardinfolist == null)
                {
                    m_Cardinfolist = contentPane.GetChildByPath("Cardinfolist") as GList;
                }
                return m_Cardinfolist;
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
    }
}
