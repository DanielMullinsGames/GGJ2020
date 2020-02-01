
namespace Peppermint.DataBinding
{
    public static class AotUtility
    {
        public static void RegisterProperty<TObject, TValue>()
        {
            DelegateUtility.CreateGetterImpl<TObject, TValue>(null);
            DelegateUtility.CreateSetterImpl<TObject, TValue>(null);
        }

        public static void RegisterImplicitOperator<T1, T2>()
        {
            DelegateUtility.CreateImplicitOperatorImpl<T1, T2>(null);
        }
    }
}
