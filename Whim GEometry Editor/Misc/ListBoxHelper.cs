using System.Collections.Generic;
using System.Windows.Controls;
internal static class ListBoxHelper
{
    public static List<int> GetItemsAsInts(ListBox box, bool selected = false)
    {
        List<int> result = new List<int>();
        if (selected)
        {
            foreach (object item in box.SelectedItems)
            {
                ListBoxItem listItem = (ListBoxItem)item;
                if (int.TryParse(listItem.Content.ToString(), out int intValue))
                {
                    result.Add(intValue);
                }
            }
        }
        else
        {
            foreach (object item in box.Items)
            {
                ListBoxItem listItem = (ListBoxItem)item;
                if (int.TryParse(listItem.Content.ToString(), out int intValue))
                {
                    result.Add(intValue);
                }
            }
        }
        return result;
    }
    public static List<string> GetItemsAsStrings(ListBox box, bool selected = false)
    {
        List<string> result = new List<string>();
        if (selected)
        {
            foreach (object item in box.SelectedItems)
            {
                ListBoxItem listItem = (ListBoxItem)item;
                if (listItem.Content != null)
                {
                    result.Add(listItem.Content.ToString());
                }
            }
        }
        else
        {
            foreach (object item in box.Items)
            {
                ListBoxItem listItem = (ListBoxItem)item;
                if (listItem.Content != null)
                {
                    result.Add(listItem.Content.ToString());
                }
            }
        }
        return result;
    }
    internal static void ChangeListBoxSelection(ListBox listBox, bool selectAll, bool selectNone, bool invertSelection)
    {
        if (selectAll)
        {
            foreach (object item in listBox.Items)
            {
                listBox.SelectedItems.Add(item);
            }
        }
        else if (selectNone)
        {
            listBox.SelectedItems.Clear();
        }
        else if (invertSelection)
        {
            foreach (object item in listBox.Items)
            {
                if (listBox.SelectedItems.Contains(item))
                {
                    listBox.SelectedItems.Remove(item);
                }
                else
                {
                    listBox.SelectedItems.Add(item);
                }
            }
        }
    }
   internal static string GetName(ListBox target)
    {
        ListBoxItem item = target.SelectedItems[0] as ListBoxItem;
        return item.Content.ToString();
    }
}
