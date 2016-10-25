
namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public struct Margin
	{
		/// <summary>
		/// 
		/// </summary>
		public int left;
		
		/// <summary>
		/// 
		/// </summary>
		public int right;

		/// <summary>
		/// 
		/// </summary>
		public int top;

		/// <summary>
		/// 
		/// </summary>
		public int bottom;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		public void Parse(string str)
		{
			if (str == null)
			{
				left = right = top = bottom = 0;
				return;
			}

			string[] arr = str.Split(',');
			if (arr.Length <= 1)
			{
				int k = int.Parse(arr[0]);
				top = k;
				bottom = k;
				left = k;
				right = k;
			}
			else
			{
				top = int.Parse(arr[0]);
				bottom = int.Parse(arr[1]);
				left = int.Parse(arr[2]);
				right = int.Parse(arr[3]);
			}
		}
	}
}
