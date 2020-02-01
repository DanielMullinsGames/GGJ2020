using System;

namespace Peppermint.DataBinding
{
    /// <summary>
    /// A simple bit array implementation.
    /// </summary>
    public class BitArraySlim
    {
        private int[] dataArray;
        private int data;
        private int length;

        private bool UseDataArray
        {
            get { return dataArray != null; }
        }

        public int Length
        {
            get { return length; }
        }
        
        public BitArraySlim(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (length > 32)
            {
                // create data array
                int n = (length + 31) / 32;
                dataArray = new int[n];
            }

            this.length = length;
        }

        public bool this[int index]
        {
            get
            {
                return Get(index);
            }

            set
            {
                Set(index, value);
            }
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index >= length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (UseDataArray)
            {
                SetToDataArray(index, value);
            }
            else
            {
                SetToData(index, value);
            }
        }

        public bool Get(int index)
        {
            if (index < 0 || index >= length)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (UseDataArray)
            {
                return GetFromDataArray(index);
            }
            else
            {
                return GetFromData(index);
            }
        }

        public void SetAll(bool value)
        {
            if (UseDataArray)
            {
                SetAllToDataArray(value);
            }
            else
            {
                SetAllToData(value);
            }
        }

        private void SetToDataArray(int index, bool value)
        {
            int dataIndex = index / 32;
            int bitIndex = index % 32;
            int mask = (1 << bitIndex);

            if (value)
            {
                dataArray[dataIndex] |= mask;
            }
            else
            {
                mask = ~mask;
                dataArray[dataIndex] &= mask;
            }
        }

        private bool GetFromDataArray(int index)
        {
            int dataIndex = index / 32;
            int bitIndex = index % 32;
            int mask = (1 << bitIndex);

            return (dataArray[dataIndex] & mask) != 0;
        }

        private void SetAllToDataArray(bool value)
        {
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (value)
                {
                    dataArray[i] = -1;
                }
                else
                {
                    dataArray[i] = 0;
                }
            }
        }

        private void SetToData(int index, bool value)
        {
            int bitIndex = index % 32;
            int mask = (1 << bitIndex);

            if (value)
            {
                data |= mask;
            }
            else
            {
                mask = ~mask;
                data &= mask;
            }
        }

        private bool GetFromData(int index)
        {
            int bitIndex = index % 32;
            int mask = (1 << bitIndex);

            return (data & mask) != 0;
        }

        private void SetAllToData(bool value)
        {
            if (value)
            {
                data = -1;
            }
            else
            {
                data = 0;
            }
        }
    }
}
