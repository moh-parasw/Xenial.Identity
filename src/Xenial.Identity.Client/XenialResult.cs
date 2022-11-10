using System;
using System.Collections.Generic;
using System.Text;

namespace Xenial.Identity.Client;

#if NET7_0_OR_GREATER
[JsonDerivedType(typeof(Success), typeDiscriminator: nameof(Success))]
[JsonDerivedType(typeof(Error), typeDiscriminator: nameof(Error))]
#endif
[Dunet.Union]
public abstract partial record XenialResult<TData>
{
    public partial record Success(TData Data);
    public partial record Error(Exception Exception);

    public TData Unwrap() => Match(m => m.Data, e => throw e.Exception);
}
