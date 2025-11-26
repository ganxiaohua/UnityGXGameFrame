# UnityGXGameFrame
高性能组件式框架,参考ET,GF,Entitas框架

* 需要odin插件 yooassets插件 unitask插件 fgui插件
* 测试案例：https://github.com/ganxiaohua/GXGame
* 详情知乎：https://www.zhihu.com/column/c_1841845301331226626

集成UI框架(基于fgui),资源加载框架(基于yooAssets)

本框架主要包含三部分主城，1.类似unity的组件系统2.类似双影奇境的ECC模式

特点:

  1.可以做到逻辑和渲染完全分离,逻辑端只需要关注数据即可,即便是像是位移这种需求,也不需要等待模型加载完成,直接设置位置即可,模型创建完成会自动修改为你设置的位置，这是游戏中的一个主角,他的拥有那些组件，以及拥有的能力一览无余
    <img width="1228" height="510" alt="image" src="https://github.com/user-attachments/assets/28d3f9ca-e8cc-4240-b928-977e8a50960e" />
    <img width="822" height="567" alt="image" src="https://github.com/user-attachments/assets/ce654ffe-181e-4830-9f2e-72c4a3b61488" />
    <img width="416" height="353" alt="image" src="https://github.com/user-attachments/assets/df3ce69e-d41f-4bdd-873c-bf5986e31f46" />

  2.全游戏只有一份的GameObject数据类,WorldPos ,LocalPos ,LocalScale,WorldScale,WorldRot,localRot 等等只有一个源头.

  3.强大的UI系统,入场动画,出场动画,等各种期间的调用都可以,且顺序完全按照你调用的方式进行,允许打开之前进行资源加载和通讯等耗时操作,且两个界面完全连续,以及UI的延迟删除复用.

  4.采用YooAssets,基础拓展功能已经加入,需要定制化自行拓展

  5.基于Entitas的设计,在使用ecs系统的时候绝大多数都是数组操作,所以响应极快.

  6.所见即所得,生命周期接口以及事件接口,想要什么事件自己创建接口就行,顺带一体,事件中去除其他的事件或者删除自己也是不会有问题的.

    ```
    public abstract class UIEntity : Entity, IStartSystem, IPreShowSystem, IShowSystem, IHideSystem, IUpdateSystem

    public class UICardListWindow : UIEntity, ITestEvent
    ```
   7.全方面的editor工具提供,包含打包,事件绑定,ecs绑定,实体审查工具,如果你使用rider就可以在ilviewer中查看c#的il代码或者 低等高等代码.

   8.如果你实在不想用ecs部分的系统,可以使用行为机(内置)

 编辑器功能 看图识功能:
 
![image](https://github.com/ganxiaohua/UnityGXGameFrame/assets/20961017/0f62a9d7-775b-4889-a289-7549f9911910)

![image](https://github.com/ganxiaohua/UnityGXGameFrame/assets/20961017/31555142-6b3d-47f9-b155-1e8f80f5428a)

![image](https://github.com/ganxiaohua/UnityGXGameFrame/assets/20961017/4b1a769b-fb88-4bd9-bc76-ad875bcbd148)

![image](https://github.com/user-attachments/assets/66ab3099-f3fc-4ca1-8c9e-b89b213bf31e)

 

