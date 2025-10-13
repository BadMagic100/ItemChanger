using System;
using ItemChanger.Items;

namespace ItemChanger.Events.Args;

public class GetItemEventArgs(string itemName) : EventArgs
{
    public string ItemName => itemName;
    public Item? Current { get; set; }
}
