using System.ComponentModel;

namespace SYLOGOS.Util
{
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool isSorted;
        private ListSortDirection sortDirection;
        private PropertyDescriptor? sortProperty;

        public SortableBindingList() : base()
        {
        }

        public SortableBindingList(IList<T> list) : base(list)
        {
        }

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => isSorted;

        protected override ListSortDirection SortDirectionCore => sortDirection;
        protected override PropertyDescriptor? SortPropertyCore => sortProperty;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            List<T> itemsList = (List<T>)Items;
            itemsList.Sort((a, b) =>
            {
                object? aValue = prop.GetValue(a);
                object? bValue = prop.GetValue(b);
                return direction == ListSortDirection.Ascending
                    ? Comparer<object>.Default.Compare(aValue, bValue)
                    : Comparer<object>.Default.Compare(bValue, aValue);
            });

            sortDirection = direction;
            sortProperty = prop;
            isSorted = true;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            isSorted = false;
        }
    }
}
