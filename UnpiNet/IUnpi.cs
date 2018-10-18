using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnpiNet
{
    interface IUnpi
    {
        Stream InputStream { get; }
        Stream OutputStream { get; }

        void Close();
        void Open();
    }
}
