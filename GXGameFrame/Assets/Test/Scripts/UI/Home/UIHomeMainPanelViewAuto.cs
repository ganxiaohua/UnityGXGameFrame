using FairyGUI;
using GameFrame;

namespace GXGame
{
    public partial class UIHomeMainPanelView : UIViewBase
    {
        private GComponent m_Bg;

        private GComponent Bg
        {
            get
            {
                if (m_Bg == null)
                {
                    m_Bg = contentPane.GetChildByPath("Bg") as GComponent;
                }

                return m_Bg;
            }
        }

        private GImage m_Bg_Bg;

        private GImage Bg_Bg
        {
            get
            {
                if (m_Bg_Bg == null)
                {
                    m_Bg_Bg = contentPane.GetChildByPath("Bg.Bg") as GImage;
                }

                return m_Bg_Bg;
            }
        }

        private GLoader m_Bg_Head;

        private GLoader Bg_Head
        {
            get
            {
                if (m_Bg_Head == null)
                {
                    m_Bg_Head = contentPane.GetChildByPath("Bg.Head") as GLoader;
                }

                return m_Bg_Head;
            }
        }

        private GLoader m_Bg_HeadFrame;

        private GLoader Bg_HeadFrame
        {
            get
            {
                if (m_Bg_HeadFrame == null)
                {
                    m_Bg_HeadFrame = contentPane.GetChildByPath("Bg.HeadFrame") as GLoader;
                }

                return m_Bg_HeadFrame;
            }
        }

        private GComponent m_Bg_AssetsButton01;

        private GComponent Bg_AssetsButton01
        {
            get
            {
                if (m_Bg_AssetsButton01 == null)
                {
                    m_Bg_AssetsButton01 = contentPane.GetChildByPath("Bg.AssetsButton01") as GComponent;
                }

                return m_Bg_AssetsButton01;
            }
        }

        private GButton m_Bg_AssetsButton01_AssetsAddBtn;

        private GButton Bg_AssetsButton01_AssetsAddBtn
        {
            get
            {
                if (m_Bg_AssetsButton01_AssetsAddBtn == null)
                {
                    m_Bg_AssetsButton01_AssetsAddBtn = contentPane.GetChildByPath("Bg.AssetsButton01.AssetsAddBtn") as GButton;
                }

                return m_Bg_AssetsButton01_AssetsAddBtn;
            }
        }

        private GImage m_Bg_AssetsButton01_AssetsAddBtn_n0;

        private GImage Bg_AssetsButton01_AssetsAddBtn_n0
        {
            get
            {
                if (m_Bg_AssetsButton01_AssetsAddBtn_n0 == null)
                {
                    m_Bg_AssetsButton01_AssetsAddBtn_n0 = contentPane.GetChildByPath("Bg.AssetsButton01.AssetsAddBtn.n0") as GImage;
                }

                return m_Bg_AssetsButton01_AssetsAddBtn_n0;
            }
        }

        private GTextField m_Bg_AssetsButton01_NumberTxt;

        private GTextField Bg_AssetsButton01_NumberTxt
        {
            get
            {
                if (m_Bg_AssetsButton01_NumberTxt == null)
                {
                    m_Bg_AssetsButton01_NumberTxt = contentPane.GetChildByPath("Bg.AssetsButton01.NumberTxt") as GTextField;
                }

                return m_Bg_AssetsButton01_NumberTxt;
            }
        }

        private GLoader m_Bg_AssetsButton01_AssetsSmallIconLoader;

        private GLoader Bg_AssetsButton01_AssetsSmallIconLoader
        {
            get
            {
                if (m_Bg_AssetsButton01_AssetsSmallIconLoader == null)
                {
                    m_Bg_AssetsButton01_AssetsSmallIconLoader = contentPane.GetChildByPath("Bg.AssetsButton01.AssetsSmallIconLoader") as GLoader;
                }

                return m_Bg_AssetsButton01_AssetsSmallIconLoader;
            }
        }

        private GComponent m_Bg_AssetsButton02;

        private GComponent Bg_AssetsButton02
        {
            get
            {
                if (m_Bg_AssetsButton02 == null)
                {
                    m_Bg_AssetsButton02 = contentPane.GetChildByPath("Bg.AssetsButton02") as GComponent;
                }

                return m_Bg_AssetsButton02;
            }
        }

        private GButton m_Bg_AssetsButton02_AssetsAddBtn;

        private GButton Bg_AssetsButton02_AssetsAddBtn
        {
            get
            {
                if (m_Bg_AssetsButton02_AssetsAddBtn == null)
                {
                    m_Bg_AssetsButton02_AssetsAddBtn = contentPane.GetChildByPath("Bg.AssetsButton02.AssetsAddBtn") as GButton;
                }

                return m_Bg_AssetsButton02_AssetsAddBtn;
            }
        }

        private GImage m_Bg_AssetsButton02_AssetsAddBtn_n0;

        private GImage Bg_AssetsButton02_AssetsAddBtn_n0
        {
            get
            {
                if (m_Bg_AssetsButton02_AssetsAddBtn_n0 == null)
                {
                    m_Bg_AssetsButton02_AssetsAddBtn_n0 = contentPane.GetChildByPath("Bg.AssetsButton02.AssetsAddBtn.n0") as GImage;
                }

                return m_Bg_AssetsButton02_AssetsAddBtn_n0;
            }
        }

        private GTextField m_Bg_AssetsButton02_NumberTxt;

        private GTextField Bg_AssetsButton02_NumberTxt
        {
            get
            {
                if (m_Bg_AssetsButton02_NumberTxt == null)
                {
                    m_Bg_AssetsButton02_NumberTxt = contentPane.GetChildByPath("Bg.AssetsButton02.NumberTxt") as GTextField;
                }

                return m_Bg_AssetsButton02_NumberTxt;
            }
        }

        private GLoader m_Bg_AssetsButton02_AssetsSmallIconLoader;

        private GLoader Bg_AssetsButton02_AssetsSmallIconLoader
        {
            get
            {
                if (m_Bg_AssetsButton02_AssetsSmallIconLoader == null)
                {
                    m_Bg_AssetsButton02_AssetsSmallIconLoader = contentPane.GetChildByPath("Bg.AssetsButton02.AssetsSmallIconLoader") as GLoader;
                }

                return m_Bg_AssetsButton02_AssetsSmallIconLoader;
            }
        }

        private GImage m_Bg_FightIcon;

        private GImage Bg_FightIcon
        {
            get
            {
                if (m_Bg_FightIcon == null)
                {
                    m_Bg_FightIcon = contentPane.GetChildByPath("Bg.FightIcon") as GImage;
                }

                return m_Bg_FightIcon;
            }
        }

        private GImage m_Bg_LvBg;

        private GImage Bg_LvBg
        {
            get
            {
                if (m_Bg_LvBg == null)
                {
                    m_Bg_LvBg = contentPane.GetChildByPath("Bg.LvBg") as GImage;
                }

                return m_Bg_LvBg;
            }
        }

        private GTextField m_Bg_LvTxt;

        private GTextField Bg_LvTxt
        {
            get
            {
                if (m_Bg_LvTxt == null)
                {
                    m_Bg_LvTxt = contentPane.GetChildByPath("Bg.LvTxt") as GTextField;
                }

                return m_Bg_LvTxt;
            }
        }

        private GTextField m_Bg_NameTxt;

        private GTextField Bg_NameTxt
        {
            get
            {
                if (m_Bg_NameTxt == null)
                {
                    m_Bg_NameTxt = contentPane.GetChildByPath("Bg.NameTxt") as GTextField;
                }

                return m_Bg_NameTxt;
            }
        }

        private GTextField m_Bg_FightTxt;

        private GTextField Bg_FightTxt
        {
            get
            {
                if (m_Bg_FightTxt == null)
                {
                    m_Bg_FightTxt = contentPane.GetChildByPath("Bg.FightTxt") as GTextField;
                }

                return m_Bg_FightTxt;
            }
        }

        private GComponent m_PicList;

        private GComponent PicList
        {
            get
            {
                if (m_PicList == null)
                {
                    m_PicList = contentPane.GetChildByPath("PicList") as GComponent;
                }

                return m_PicList;
            }
        }

        private GImage m_PicList_Bg;

        private GImage PicList_Bg
        {
            get
            {
                if (m_PicList_Bg == null)
                {
                    m_PicList_Bg = contentPane.GetChildByPath("PicList.Bg") as GImage;
                }

                return m_PicList_Bg;
            }
        }

        private GList m_PicList_PicList;

        private GList PicList_PicList
        {
            get
            {
                if (m_PicList_PicList == null)
                {
                    m_PicList_PicList = contentPane.GetChildByPath("PicList.PicList") as GList;
                }

                return m_PicList_PicList;
            }
        }

        private GImage m_PicList_BannerBg;

        private GImage PicList_BannerBg
        {
            get
            {
                if (m_PicList_BannerBg == null)
                {
                    m_PicList_BannerBg = contentPane.GetChildByPath("PicList.BannerBg") as GImage;
                }

                return m_PicList_BannerBg;
            }
        }

        private GImage m_PicList_Huawen;

        private GImage PicList_Huawen
        {
            get
            {
                if (m_PicList_Huawen == null)
                {
                    m_PicList_Huawen = contentPane.GetChildByPath("PicList.Huawen") as GImage;
                }

                return m_PicList_Huawen;
            }
        }

        private GList m_PicList_PageNumberList;

        private GList PicList_PageNumberList
        {
            get
            {
                if (m_PicList_PageNumberList == null)
                {
                    m_PicList_PageNumberList = contentPane.GetChildByPath("PicList.PageNumberList") as GList;
                }

                return m_PicList_PageNumberList;
            }
        }

        private GButton m_BtnActivity;

        private GButton BtnActivity
        {
            get
            {
                if (m_BtnActivity == null)
                {
                    m_BtnActivity = contentPane.GetChildByPath("BtnActivity") as GButton;
                }

                return m_BtnActivity;
            }
        }

        private GImage m_BtnActivity_norm;

        private GImage BtnActivity_norm
        {
            get
            {
                if (m_BtnActivity_norm == null)
                {
                    m_BtnActivity_norm = contentPane.GetChildByPath("BtnActivity.norm") as GImage;
                }

                return m_BtnActivity_norm;
            }
        }

        private GImage m_BtnActivity_icon;

        private GImage BtnActivity_icon
        {
            get
            {
                if (m_BtnActivity_icon == null)
                {
                    m_BtnActivity_icon = contentPane.GetChildByPath("BtnActivity.icon") as GImage;
                }

                return m_BtnActivity_icon;
            }
        }

        private GButton m_BtnFriend;

        private GButton BtnFriend
        {
            get
            {
                if (m_BtnFriend == null)
                {
                    m_BtnFriend = contentPane.GetChildByPath("BtnFriend") as GButton;
                }

                return m_BtnFriend;
            }
        }

        private GImage m_BtnFriend_norm;

        private GImage BtnFriend_norm
        {
            get
            {
                if (m_BtnFriend_norm == null)
                {
                    m_BtnFriend_norm = contentPane.GetChildByPath("BtnFriend.norm") as GImage;
                }

                return m_BtnFriend_norm;
            }
        }

        private GImage m_BtnFriend_icon;

        private GImage BtnFriend_icon
        {
            get
            {
                if (m_BtnFriend_icon == null)
                {
                    m_BtnFriend_icon = contentPane.GetChildByPath("BtnFriend.icon") as GImage;
                }

                return m_BtnFriend_icon;
            }
        }

        private GButton m_BtnMail;

        private GButton BtnMail
        {
            get
            {
                if (m_BtnMail == null)
                {
                    m_BtnMail = contentPane.GetChildByPath("BtnMail") as GButton;
                }

                return m_BtnMail;
            }
        }

        private GImage m_BtnMail_norm;

        private GImage BtnMail_norm
        {
            get
            {
                if (m_BtnMail_norm == null)
                {
                    m_BtnMail_norm = contentPane.GetChildByPath("BtnMail.norm") as GImage;
                }

                return m_BtnMail_norm;
            }
        }

        private GImage m_BtnMail_icon;

        private GImage BtnMail_icon
        {
            get
            {
                if (m_BtnMail_icon == null)
                {
                    m_BtnMail_icon = contentPane.GetChildByPath("BtnMail.icon") as GImage;
                }

                return m_BtnMail_icon;
            }
        }

        private GButton m_BtnTalk;

        private GButton BtnTalk
        {
            get
            {
                if (m_BtnTalk == null)
                {
                    m_BtnTalk = contentPane.GetChildByPath("BtnTalk") as GButton;
                }

                return m_BtnTalk;
            }
        }

        private GImage m_BtnTalk_norm;

        private GImage BtnTalk_norm
        {
            get
            {
                if (m_BtnTalk_norm == null)
                {
                    m_BtnTalk_norm = contentPane.GetChildByPath("BtnTalk.norm") as GImage;
                }

                return m_BtnTalk_norm;
            }
        }

        private GImage m_BtnTalk_icon;

        private GImage BtnTalk_icon
        {
            get
            {
                if (m_BtnTalk_icon == null)
                {
                    m_BtnTalk_icon = contentPane.GetChildByPath("BtnTalk.icon") as GImage;
                }

                return m_BtnTalk_icon;
            }
        }

        private GButton m_n41;

        private GButton n41
        {
            get
            {
                if (m_n41 == null)
                {
                    m_n41 = contentPane.GetChildByPath("n41") as GButton;
                }

                return m_n41;
            }
        }

        private GImage m_n41_norm;

        private GImage n41_norm
        {
            get
            {
                if (m_n41_norm == null)
                {
                    m_n41_norm = contentPane.GetChildByPath("n41.norm") as GImage;
                }

                return m_n41_norm;
            }
        }

        private GImage m_n41_icon;

        private GImage n41_icon
        {
            get
            {
                if (m_n41_icon == null)
                {
                    m_n41_icon = contentPane.GetChildByPath("n41.icon") as GImage;
                }

                return m_n41_icon;
            }
        }

        private GButton m_TopBtnHide;

        private GButton TopBtnHide
        {
            get
            {
                if (m_TopBtnHide == null)
                {
                    m_TopBtnHide = contentPane.GetChildByPath("TopBtnHide") as GButton;
                }

                return m_TopBtnHide;
            }
        }

        private GImage m_TopBtnHide_Icon;

        private GImage TopBtnHide_Icon
        {
            get
            {
                if (m_TopBtnHide_Icon == null)
                {
                    m_TopBtnHide_Icon = contentPane.GetChildByPath("TopBtnHide.Icon") as GImage;
                }

                return m_TopBtnHide_Icon;
            }
        }

        private GButton m_TopBtnBG;

        private GButton TopBtnBG
        {
            get
            {
                if (m_TopBtnBG == null)
                {
                    m_TopBtnBG = contentPane.GetChildByPath("TopBtnBG") as GButton;
                }

                return m_TopBtnBG;
            }
        }

        private GImage m_TopBtnBG_Icon;

        private GImage TopBtnBG_Icon
        {
            get
            {
                if (m_TopBtnBG_Icon == null)
                {
                    m_TopBtnBG_Icon = contentPane.GetChildByPath("TopBtnBG.Icon") as GImage;
                }

                return m_TopBtnBG_Icon;
            }
        }

        private GButton m_TopBtnCard;

        private GButton TopBtnCard
        {
            get
            {
                if (m_TopBtnCard == null)
                {
                    m_TopBtnCard = contentPane.GetChildByPath("TopBtnCard") as GButton;
                }

                return m_TopBtnCard;
            }
        }

        private GImage m_TopBtnCard_Icon;

        private GImage TopBtnCard_Icon
        {
            get
            {
                if (m_TopBtnCard_Icon == null)
                {
                    m_TopBtnCard_Icon = contentPane.GetChildByPath("TopBtnCard.Icon") as GImage;
                }

                return m_TopBtnCard_Icon;
            }
        }

        private GButton m_TopBtnNotice;

        private GButton TopBtnNotice
        {
            get
            {
                if (m_TopBtnNotice == null)
                {
                    m_TopBtnNotice = contentPane.GetChildByPath("TopBtnNotice") as GButton;
                }

                return m_TopBtnNotice;
            }
        }

        private GImage m_TopBtnNotice_Icon;

        private GImage TopBtnNotice_Icon
        {
            get
            {
                if (m_TopBtnNotice_Icon == null)
                {
                    m_TopBtnNotice_Icon = contentPane.GetChildByPath("TopBtnNotice.Icon") as GImage;
                }

                return m_TopBtnNotice_Icon;
            }
        }

        private GButton m_TopBtnSetting;

        private GButton TopBtnSetting
        {
            get
            {
                if (m_TopBtnSetting == null)
                {
                    m_TopBtnSetting = contentPane.GetChildByPath("TopBtnSetting") as GButton;
                }

                return m_TopBtnSetting;
            }
        }

        private GImage m_TopBtnSetting_Icon;

        private GImage TopBtnSetting_Icon
        {
            get
            {
                if (m_TopBtnSetting_Icon == null)
                {
                    m_TopBtnSetting_Icon = contentPane.GetChildByPath("TopBtnSetting.Icon") as GImage;
                }

                return m_TopBtnSetting_Icon;
            }
        }

        private GList m_ActivityPicGrp;

        private GList ActivityPicGrp
        {
            get
            {
                if (m_ActivityPicGrp == null)
                {
                    m_ActivityPicGrp = contentPane.GetChildByPath("ActivityPicGrp") as GList;
                }

                return m_ActivityPicGrp;
            }
        }

        private GButton m_BtnContract;

        private GButton BtnContract
        {
            get
            {
                if (m_BtnContract == null)
                {
                    m_BtnContract = contentPane.GetChildByPath("BtnContract") as GButton;
                }

                return m_BtnContract;
            }
        }

        private GImage m_BtnContract_n3;

        private GImage BtnContract_n3
        {
            get
            {
                if (m_BtnContract_n3 == null)
                {
                    m_BtnContract_n3 = contentPane.GetChildByPath("BtnContract.n3") as GImage;
                }

                return m_BtnContract_n3;
            }
        }

        private GImage m_BtnContract_n4;

        private GImage BtnContract_n4
        {
            get
            {
                if (m_BtnContract_n4 == null)
                {
                    m_BtnContract_n4 = contentPane.GetChildByPath("BtnContract.n4") as GImage;
                }

                return m_BtnContract_n4;
            }
        }

        private GButton m_BtnAdventure;

        private GButton BtnAdventure
        {
            get
            {
                if (m_BtnAdventure == null)
                {
                    m_BtnAdventure = contentPane.GetChildByPath("BtnAdventure") as GButton;
                }

                return m_BtnAdventure;
            }
        }

        private GImage m_BtnAdventure_Icon;

        private GImage BtnAdventure_Icon
        {
            get
            {
                if (m_BtnAdventure_Icon == null)
                {
                    m_BtnAdventure_Icon = contentPane.GetChildByPath("BtnAdventure.Icon") as GImage;
                }

                return m_BtnAdventure_Icon;
            }
        }

        private GButton m_BtnBattle;

        private GButton BtnBattle
        {
            get
            {
                if (m_BtnBattle == null)
                {
                    m_BtnBattle = contentPane.GetChildByPath("BtnBattle") as GButton;
                }

                return m_BtnBattle;
            }
        }

        private GImage m_BtnBattle_Icon;

        private GImage BtnBattle_Icon
        {
            get
            {
                if (m_BtnBattle_Icon == null)
                {
                    m_BtnBattle_Icon = contentPane.GetChildByPath("BtnBattle.Icon") as GImage;
                }

                return m_BtnBattle_Icon;
            }
        }

        private GButton m_BtnHome;

        private GButton BtnHome
        {
            get
            {
                if (m_BtnHome == null)
                {
                    m_BtnHome = contentPane.GetChildByPath("BtnHome") as GButton;
                }

                return m_BtnHome;
            }
        }

        private GImage m_BtnHome_Icon;

        private GImage BtnHome_Icon
        {
            get
            {
                if (m_BtnHome_Icon == null)
                {
                    m_BtnHome_Icon = contentPane.GetChildByPath("BtnHome.Icon") as GImage;
                }

                return m_BtnHome_Icon;
            }
        }

        private GComponent m_BtnDevelop;

        private GComponent BtnDevelop
        {
            get
            {
                if (m_BtnDevelop == null)
                {
                    m_BtnDevelop = contentPane.GetChildByPath("BtnDevelop") as GComponent;
                }

                return m_BtnDevelop;
            }
        }

        private GButton m_BtnDevelop_BtnDevelop;

        private GButton BtnDevelop_BtnDevelop
        {
            get
            {
                if (m_BtnDevelop_BtnDevelop == null)
                {
                    m_BtnDevelop_BtnDevelop = contentPane.GetChildByPath("BtnDevelop.BtnDevelop") as GButton;
                }

                return m_BtnDevelop_BtnDevelop;
            }
        }

        private GImage m_BtnDevelop_BtnDevelop_Bg;

        private GImage BtnDevelop_BtnDevelop_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnDevelop_Bg == null)
                {
                    m_BtnDevelop_BtnDevelop_Bg = contentPane.GetChildByPath("BtnDevelop.BtnDevelop.Bg") as GImage;
                }

                return m_BtnDevelop_BtnDevelop_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnDevelop_Icon;

        private GImage BtnDevelop_BtnDevelop_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnDevelop_Icon == null)
                {
                    m_BtnDevelop_BtnDevelop_Icon = contentPane.GetChildByPath("BtnDevelop.BtnDevelop.Icon") as GImage;
                }

                return m_BtnDevelop_BtnDevelop_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnPotency;

        private GButton BtnDevelop_BtnPotency
        {
            get
            {
                if (m_BtnDevelop_BtnPotency == null)
                {
                    m_BtnDevelop_BtnPotency = contentPane.GetChildByPath("BtnDevelop.BtnPotency") as GButton;
                }

                return m_BtnDevelop_BtnPotency;
            }
        }

        private GImage m_BtnDevelop_BtnPotency_Bg;

        private GImage BtnDevelop_BtnPotency_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnPotency_Bg == null)
                {
                    m_BtnDevelop_BtnPotency_Bg = contentPane.GetChildByPath("BtnDevelop.BtnPotency.Bg") as GImage;
                }

                return m_BtnDevelop_BtnPotency_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnPotency_Icon;

        private GImage BtnDevelop_BtnPotency_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnPotency_Icon == null)
                {
                    m_BtnDevelop_BtnPotency_Icon = contentPane.GetChildByPath("BtnDevelop.BtnPotency.Icon") as GImage;
                }

                return m_BtnDevelop_BtnPotency_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnRecruit;

        private GButton BtnDevelop_BtnRecruit
        {
            get
            {
                if (m_BtnDevelop_BtnRecruit == null)
                {
                    m_BtnDevelop_BtnRecruit = contentPane.GetChildByPath("BtnDevelop.BtnRecruit") as GButton;
                }

                return m_BtnDevelop_BtnRecruit;
            }
        }

        private GImage m_BtnDevelop_BtnRecruit_Bg;

        private GImage BtnDevelop_BtnRecruit_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnRecruit_Bg == null)
                {
                    m_BtnDevelop_BtnRecruit_Bg = contentPane.GetChildByPath("BtnDevelop.BtnRecruit.Bg") as GImage;
                }

                return m_BtnDevelop_BtnRecruit_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnRecruit_Icon;

        private GImage BtnDevelop_BtnRecruit_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnRecruit_Icon == null)
                {
                    m_BtnDevelop_BtnRecruit_Icon = contentPane.GetChildByPath("BtnDevelop.BtnRecruit.Icon") as GImage;
                }

                return m_BtnDevelop_BtnRecruit_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnBag;

        private GButton BtnDevelop_BtnBag
        {
            get
            {
                if (m_BtnDevelop_BtnBag == null)
                {
                    m_BtnDevelop_BtnBag = contentPane.GetChildByPath("BtnDevelop.BtnBag") as GButton;
                }

                return m_BtnDevelop_BtnBag;
            }
        }

        private GImage m_BtnDevelop_BtnBag_Bg;

        private GImage BtnDevelop_BtnBag_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnBag_Bg == null)
                {
                    m_BtnDevelop_BtnBag_Bg = contentPane.GetChildByPath("BtnDevelop.BtnBag.Bg") as GImage;
                }

                return m_BtnDevelop_BtnBag_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnBag_Icon;

        private GImage BtnDevelop_BtnBag_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnBag_Icon == null)
                {
                    m_BtnDevelop_BtnBag_Icon = contentPane.GetChildByPath("BtnDevelop.BtnBag.Icon") as GImage;
                }

                return m_BtnDevelop_BtnBag_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnSociety;

        private GButton BtnDevelop_BtnSociety
        {
            get
            {
                if (m_BtnDevelop_BtnSociety == null)
                {
                    m_BtnDevelop_BtnSociety = contentPane.GetChildByPath("BtnDevelop.BtnSociety") as GButton;
                }

                return m_BtnDevelop_BtnSociety;
            }
        }

        private GImage m_BtnDevelop_BtnSociety_Bg;

        private GImage BtnDevelop_BtnSociety_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnSociety_Bg == null)
                {
                    m_BtnDevelop_BtnSociety_Bg = contentPane.GetChildByPath("BtnDevelop.BtnSociety.Bg") as GImage;
                }

                return m_BtnDevelop_BtnSociety_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnSociety_Icon;

        private GImage BtnDevelop_BtnSociety_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnSociety_Icon == null)
                {
                    m_BtnDevelop_BtnSociety_Icon = contentPane.GetChildByPath("BtnDevelop.BtnSociety.Icon") as GImage;
                }

                return m_BtnDevelop_BtnSociety_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnTask;

        private GButton BtnDevelop_BtnTask
        {
            get
            {
                if (m_BtnDevelop_BtnTask == null)
                {
                    m_BtnDevelop_BtnTask = contentPane.GetChildByPath("BtnDevelop.BtnTask") as GButton;
                }

                return m_BtnDevelop_BtnTask;
            }
        }

        private GImage m_BtnDevelop_BtnTask_Bg;

        private GImage BtnDevelop_BtnTask_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnTask_Bg == null)
                {
                    m_BtnDevelop_BtnTask_Bg = contentPane.GetChildByPath("BtnDevelop.BtnTask.Bg") as GImage;
                }

                return m_BtnDevelop_BtnTask_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnTask_Icon;

        private GImage BtnDevelop_BtnTask_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnTask_Icon == null)
                {
                    m_BtnDevelop_BtnTask_Icon = contentPane.GetChildByPath("BtnDevelop.BtnTask.Icon") as GImage;
                }

                return m_BtnDevelop_BtnTask_Icon;
            }
        }

        private GButton m_BtnDevelop_BtnShop;

        private GButton BtnDevelop_BtnShop
        {
            get
            {
                if (m_BtnDevelop_BtnShop == null)
                {
                    m_BtnDevelop_BtnShop = contentPane.GetChildByPath("BtnDevelop.BtnShop") as GButton;
                }

                return m_BtnDevelop_BtnShop;
            }
        }

        private GImage m_BtnDevelop_BtnShop_Bg;

        private GImage BtnDevelop_BtnShop_Bg
        {
            get
            {
                if (m_BtnDevelop_BtnShop_Bg == null)
                {
                    m_BtnDevelop_BtnShop_Bg = contentPane.GetChildByPath("BtnDevelop.BtnShop.Bg") as GImage;
                }

                return m_BtnDevelop_BtnShop_Bg;
            }
        }

        private GImage m_BtnDevelop_BtnShop_Icon;

        private GImage BtnDevelop_BtnShop_Icon
        {
            get
            {
                if (m_BtnDevelop_BtnShop_Icon == null)
                {
                    m_BtnDevelop_BtnShop_Icon = contentPane.GetChildByPath("BtnDevelop.BtnShop.Icon") as GImage;
                }

                return m_BtnDevelop_BtnShop_Icon;
            }
        }
    }
}