// ***************纵梦互娱***************
// 文件名:   GComponent.cs
// 创建作者: yhchen
// 创建时间: 2022/10/21/23:34
// 功能描述:
// 重要更新:
// ***************纵梦互娱***************

namespace FairyGUI
{
    public partial class GComponent
    {
        /// <summary>
        /// Remove all children.
        /// </summary>
        public void RemoveChildren(bool dispose)
        {
            RemoveChildren(0, -1, dispose);
        }
    }
}