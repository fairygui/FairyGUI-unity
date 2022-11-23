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