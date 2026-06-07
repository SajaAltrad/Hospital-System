using System.Collections;

namespace HospitalConsoleProject;

public class SortedLinkedList<T> : IEnumerable<T> where T : IHasId
{
    private Node<T>? head;

    public bool Add(T item)
    {
        if (Find(item.Id) != null) return false;

        Node<T> newNode = new(item);
        if (head == null || item.Id < head.Data.Id)
        {
            newNode.Next = head;
            head = newNode;
            return true;
        }

        Node<T> current = head;
        while (current.Next != null && current.Next.Data.Id < item.Id)
            current = current.Next;

        newNode.Next = current.Next;
        current.Next = newNode;
        return true;
    }

    public bool Remove(int id)
    {
        if (head == null) return false;
        if (head.Data.Id == id)
        {
            head = head.Next;
            return true;
        }

        Node<T> current = head;
        while (current.Next != null && current.Next.Data.Id != id)
            current = current.Next;

        if (current.Next == null) return false;
        current.Next = current.Next.Next;
        return true;
    }

    public T? Find(int id)
    {
        Node<T>? current = head;
        while (current != null)
        {
            if (current.Data.Id == id) return current.Data;
            current = current.Next;
        }
        return default;
    }

    public void Clear() => head = null;

    public IEnumerator<T> GetEnumerator()
    {
        Node<T>? current = head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
