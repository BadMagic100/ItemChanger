using ItemChanger.Items;
using System;

namespace ItemChanger.Events.Args;

public class GetItemEventArgs(string itemName) : EventArgs
{
    public string ItemName { get; } = itemName;
    public Item? Current { get; set; }
}
