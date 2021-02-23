using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore.RW
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RWAutoFieldAttribute : Attribute
    {
        /// <summary>
        /// 数值组件类型
        /// </summary>
        public EditType nodeInsp;
        /// <summary>
        /// 数值组件的描述
        /// </summary>
        public string des;
        /// <summary>
        /// 数值组件的提示
        /// </summary>
        public string tips;
        /// <summary>
        /// 是否显示数值组件
        /// </summary>
        public bool isShow;
        /// <summary>
        /// 是否是编辑器字段
        /// </summary>
        public bool isEditor;

        /// <summary>
        /// 字段对应的ID
        /// </summary>
        public int fieldID;

        public RWAutoFieldAttribute()
        {

        }
        
        /// <summary>
        /// 定义字段编辑器
        /// </summary>
        /// <param name="nodeInsp">字段类型</param>
        /// <param name="des">字段描述</param>
        /// <param name="isShow">是否显示在编辑器</param>
        /// <param name="tips">在编辑器中的提示</param>
        /// <param name="isEditor">是否在UNITY_EDITOR宏定义下</param>
        public RWAutoFieldAttribute(EditType nodeInsp = EditType.Null, string des = null, bool isShow = true, string tips = null, bool isEditor = false)
        {
            this.nodeInsp = nodeInsp;
            this.des = des;
            this.tips = tips;
            this.isShow = isShow;
            this.isEditor = isEditor;
        }

        /// <summary>
        /// 定义字段编辑器
        /// </summary>
        /// <param name="des">字段描述</param>
        /// <param name="isShow">是否显示在编辑器</param>
        /// <param name="tips">在编辑器中的提示</param>
        /// <param name="isEditor">是否在UNITY_EDITOR宏定义下</param>
        public RWAutoFieldAttribute(string des = null, bool isShow = true, string tips = null, bool isEditor = false)
        {
            nodeInsp = EditType.Null;
            this.des = des;
            this.tips = tips;
            this.isShow = isShow;
            this.isEditor = isEditor;
        }

        public RWAutoFieldAttribute(bool isEditor)
        {
            this.isEditor = isEditor;
        }

        public RWAutoFieldAttribute(int fieldID = -1, string des = null)
        {
            this.fieldID = fieldID;
            this.des = des;
        }
    }

    public class RWEnumAttribute : Attribute
    {
        public string des;
        public RWEnumAttribute(string des)
        {
            this.des = des;
        }
    }

    public class NewcomerGuideEnumAttribute : RWEnumAttribute
    {
        public NewcomerGuideEnumAttribute(string des) : base(des) { }
    }

    /// <summary>
    /// 自动生成的属性，用于控制编辑器显示
    /// </summary>
    public class AutoCodeAttribute : RWAutoFieldAttribute
    {
        public AutoCodeAttribute(EditType nodeInsp = EditType.Null, string des = null, bool isShow = true, string tips = null, bool isEditor = false) : base(nodeInsp, des, isShow, tips, isEditor)
        {
        }

        public AutoCodeAttribute(string des = null, bool isShow = true, string tips = null, bool isEditor = false) : base(des, isShow, tips, isEditor)
        {
        }
    }
}