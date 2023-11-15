using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevLynx.Packaging.Models
{
    /// <summary>
    /// Bidirectional node of a doubly linked list
    /// </summary>
    internal class BiNode
    {
        private BiNode _prev;
        private BiNode _next;

        public BiNode Prev
        {
            get => _prev;
            set => _prev = value;
        }
        public BiNode Next
        {
            get => _next;
            set => _next = value;
        }

        public bool HasPrev => _prev != null;
        public bool HasNext => _next != null;
        public bool IsSingle => !HasPrev && !HasNext;

        public void InsertBefore(BiNode node)
        {
            if (node != null)
            {
                node.Prev = _prev;
                node.Next = this;
            }

            if (_prev != null)
                _prev.Next = node;

            _prev = node;
        }

        public void InsertAfter(BiNode node)
        {
            if (node != null)
            {
                node.Prev = this;
                node.Next = _next;
            }

            if (_next != null)
                _next.Prev = node;

            _next = node;
        }

        public void RemoveSelf()
        {
            if (_prev != null)
            {
                _prev.Next = _next;
            }

            if (_next != null)
            {
                _next.Prev = _prev;
            }

            _prev = _next = null;
        }

        public void RemoveNext()
        {
            if (_next == null) return;

            _next = _next.Next;

            if (_next != null)
            {
                _next.Prev = this;
            }
        }

        public void RemovePrev()
        {
            if (_prev == null) return;

            _prev = _prev.Prev;

            if (_prev != null)
            {
                _prev.Next = this;
            }
        }

        public void ReplaceSelf(BiNode node)
        {
            if (node != null)
            {
                node.Prev = _prev;
                node.Next = _next;
            }

            if (_prev != null)
            {
                _prev.Next = node;
            }

            if (_next != null)
            {
                _next.Prev = node;
            }

            _prev = _next = null;
        }
    }
}
