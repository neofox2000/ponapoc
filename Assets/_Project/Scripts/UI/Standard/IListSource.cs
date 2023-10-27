using System.Collections.Generic;

public interface IListSource
{
    IListRowSource[] sourceItems
    {
        get;
    }

    void Sort();
    void SetSource(UI_ListManager list);
    void UnsetSource(UI_ListManager list);
}