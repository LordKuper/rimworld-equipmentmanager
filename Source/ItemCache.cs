namespace EquipmentManager
{
    internal class ItemCache
    {
        protected const float UpdateTimer = 24f;
        protected readonly RimworldTime UpdateTime = new RimworldTime(-1, -1, -1);
        public virtual void Update(RimworldTime time) { }
    }
}