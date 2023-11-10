using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FairyGUI
{

    /// <summary>
    /// Helper for drag and drop model.
    /// 这是一个提供特殊拖放功能的功能类。与GObject.draggable不同，拖动开始后，他使用一个替代的Button作为拖动对象,并用Wapper显示模型。
    /// 当玩家释放鼠标/手指，目标组件会发出一个全局的onDrop事件
    /// </summary>
    public class DragModelManager
    {
        private GButton _agent;
        private GGraph _holder;
        private object _sourceData;
        private GObject _source;
        private GoWrapper _wapper;

        private static DragModelManager _inst;
        public static DragModelManager inst
        {
            get
            {
                if (_inst == null)
                    _inst = new DragModelManager();
                return _inst;
            }
        }

        public DragModelManager()
        {
            _agent = (GButton)UIObjectFactory.NewObject(ObjectType.Button);
            _agent.gameObjectName = "DragModelAgent";
            _agent.SetHome(GRoot.inst);
            _agent.touchable = false;//important
            _agent.draggable = true; //开启拖拽
            _agent.SetSize(100, 100);
            _agent.SetPivot(0f, 0f, true);//设置锚点
            _agent.sortingOrder = int.MaxValue;
            _agent.onDragEnd.Add(__dragEnd); //添加拖拽结束方法

            //添加子物体占位符
            _holder = (GGraph)UIObjectFactory.NewObject(ObjectType.Graph);
            _holder.touchable = false;//important
            _holder.SetSize(100, 100);
            _holder.SetPivot(0f, 0f, true);
            _holder.SetHome(_agent);

        }

        /// <summary>
        /// Button object for real dragging.
        /// 用于实际拖动的Button对象。你可以根据实际情况设置loader的大小，对齐等。
        /// </summary>
        public GButton dragAgent
        {
            get { return _agent; }
        }


        /// <summary>
        /// Is dragging?
        /// 返回当前是否正在拖动。
        /// </summary>
        public bool dragging
        {
            get { return _agent.parent != null; }
        }


        /// <summary>
        /// Start dragging.
        /// 开始拖动。
        /// </summary>
        /// <param name="source">Source object. This is the object which initiated the dragging</param>
        /// <param name="go">Gamobject(ModelPrefab) to be used as the dragging sign.</param>
        /// <param name="sourceData">Custom data. You can get it in the onDrop event data.</param>
        /// <param name="aniName">animationName</param>
        /// <param name="touchPointID">Copy the touchId from InputEvent to here, if has one</param>
        public void StartDrag(GObject source, GameObject go, object sourceData, string aniName = "catch", int touchPointID = -1)
        {
            if (_agent.parent != null)
                return;
            

            if (_wapper == null)
            {
                _wapper = new GoWrapper(go);
                _wapper.touchable = false;
                _holder.SetNativeObject(_wapper);
            }
            else
            {
                //设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。如果你要销毁原来的GameObject，必须自行处理
                _wapper.SetWrapTarget(go, true);
            }


            _sourceData = sourceData;
            _source = source;
            GRoot.inst.AddChild(_agent);

            _agent.AddChild(_holder);
            _agent.xy = GRoot.inst.GlobalToLocal(Stage.inst.GetTouchPosition(touchPointID));
            _agent.StartDrag(touchPointID);
        }


        /// <summary>
        /// Start dragging.
        /// 开始拖动。
        /// </summary>
        /// <param name="source">Source object. This is the object which initiated the dragging</param>
        /// <param name="go">Gamobject(ModelPrefab) to be used as the dragging sign.</param>
        /// <param name="sourceData">Custom data. You can get it in the onDrop event data.</param>
        /// <param name="scale">GameObject localScale. Used as a sizing setting for the model</param>
        /// <param name="aniName">animationName</param>
        /// <param name="touchPointID">Copy the touchId from InputEvent to here, if has one</param>
        public void StartDrag(GObject source, GameObject go, object sourceData, Vector3 scale, String aniName = "catch", int touchPointID = -1)
        {
            if (go == null)
            {
                Debug.LogError("Script DragModelManager function StartDrag drag gameObject is null");
                return;
            }

            if (_agent.parent != null)
                return;

            if (scale != null)
            {
                go.transform.localScale = scale;
            }
            go.transform.position = new Vector3(0, 0, 0);  //自动设置位置
            

            if (_wapper == null)
            {
                _wapper = new GoWrapper(go);
                _holder.SetNativeObject(_wapper);
            }
            else
            {
                //设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。如果你要销毁原来的GameObject，必须自行处理
                //UnityEngine.Object.Destroy(_wapper.wrapTarget);
                // _wapper.wrapTarget = go;
                _wapper.SetWrapTarget(go, true);
            }

            _sourceData = sourceData;
            _source = source;
            GRoot.inst.AddChild(_agent);

            _agent.AddChild(_holder);
            _agent.xy = GRoot.inst.GlobalToLocal(Stage.inst.GetTouchPosition(touchPointID));
            _agent.StartDrag(touchPointID);
        }



        /// <summary>
        /// Cancel dragging.
        /// 取消拖动。
        /// </summary>
        public void Cancel()
        {
            if (_agent.parent != null)
            {
                _agent.StopDrag();
                GRoot.inst.RemoveChild(_agent);
                _sourceData = null;
            }
        }


        /// <summary>
        /// 拖拽结束
        /// </summary>
        /// <param name="evt"></param>
        private void __dragEnd(EventContext evt)
        {
            if (_agent.parent == null) //cancelled
                return;

            GRoot.inst.RemoveChild(_agent); //移除
            UnityEngine.Object.Destroy(_wapper.wrapTarget); //自动释放拖拽目标
            object sourceData = _sourceData;
            GObject source = _source;
            _sourceData = null;
            _source = null;

            GObject obj = GRoot.inst.touchTarget; //最后点击目标
            while (obj != null)
            {
                if (obj.hasEventListeners("onDrop"))
                {
                    obj.RequestFocus();
                    obj.DispatchEvent("onDrop", sourceData, source); //广播
                    return;
                }
                else
                {
                    GRoot.inst.DispatchEvent("onDrop", sourceData, source);  //未点击到目标点，通过GRoot进行事件监听
                }
                obj = obj.parent;
            }
        }
    }
}

