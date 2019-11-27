using Cam.SmartContract.Enumerators;
using Cam.VM;

namespace Cam.SmartContract.Iterators
{
    internal interface IIterator : IEnumerator
    {
        StackItem Key();
    }
}
