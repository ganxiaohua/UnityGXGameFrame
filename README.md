# UnityGXGameFrame
高性能组件式框架,参考ET,GF,Entitas框架

* 需要odin插件
* 测试案例(将之放入到Assets目录下即可):

* https://github.com/ganxiaohua/GXGame

* 需要先运行一下GXGameFrame\Config\Excel\gen_code_bin 制作表格数据


集成UI框架(基于fgui),资源加载框架(基于AA的的拓展,可以做到一键出包,一键构建,只需要少量配置)

本框架特点:

1.可以做到逻辑和渲染完全分离,逻辑端只需要关注数据即可,即便是像是位移这种需求,也不需要等待模型加载完成,直接设置位置即可,模型创建完成会自动修改为你设置的位置.
  ```
            var map = AddChild();
            map.AddViewType(typeof(GoBaseView));
            map.AddAssetPath("Map");
            map.AddWorldPos(Vector3.zero);

            var palyer = AddChild();
            palyer.AddViewType(typeof(Go2DView));
            palyer.AddAssetPath("Assets/GXGame/Art/Runtime/Role/Player/Prefab/Player.prefab");
            palyer.AddWorldPos(new Vector3(-0.5f,0,0));
            palyer.AddMoveDirection();
            palyer.AddMoveSpeed(1);
            palyer.AddInputDirection();

            var monster = AddChild();
            monster.AddViewType(typeof(Go2DView));
            monster.AddAssetPath("Assets/GXGame/Art/Runtime/Role/Monster_002/Prefab/Monster_002.prefab");
            monster.AddWorldPos(new Vector3(5,0,-1));
            monster.AddMoveDirection();
            monster.AddMoveSpeed(1);
  ```

2.全游戏只有一份的GameObject数据类,WorldPos ,LocalPos ,LocalScale,WorldScale,WorldRot,localRot 等等只有一个源头.

3.强大的UI系统,入场动画,出场动画,等各种期间的调用都可以,且顺序完全按照你调用的方式进行,允许打开之前进行资源加载和通讯等耗时操作,且两个界面完全连续,以及UI的延迟删除复用.

4.强大的资源管理系统,脱离AA原本的设计,无论你是出单机包,还是网络包,配置基本一致,同时也满足上线整包1.0之后,更新一段时间之后,重新上线了整包2.0版本, 1.0的用户不需要删除也能和2.0的用户一致.

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

 

