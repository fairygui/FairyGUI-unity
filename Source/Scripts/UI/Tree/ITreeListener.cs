using System;
using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITreeListener
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		GComponent TreeNodeCreateCell(TreeNode node);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="obj"></param>
		void TreeNodeRender(TreeNode node, GComponent obj);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="expand"></param>
		void TreeNodeWillExpand(TreeNode node, bool expand);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="context"></param>
		void TreeNodeClick(TreeNode node, EventContext context);
	}
}
