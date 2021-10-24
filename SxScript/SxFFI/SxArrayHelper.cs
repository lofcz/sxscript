namespace SxScript.SxFFI;

public static class SxArrayHelper
{
    public static void PerformSetArray(object? source, string name, object? value = null, SxToken? op = null, List<object?>? resolvedArrayExpression = null)
    {
        if (resolvedArrayExpression == null || source == null)
        {
            return;
        }
        
        if (source is SxArray dict)
        {
            if (resolvedArrayExpression.Count == 1)
            {
                object? resolvedVal = SxArithmetic.ResolveSetValue(dict.GetValue(resolvedArrayExpression[0]), value, op);
                dict.Set(resolvedArrayExpression[0], resolvedVal);
            }
            else
            {
                object? resolvedVal = SxArithmetic.ResolveSetValue(dict.GetValueChained(resolvedArrayExpression), value, op);
                dict.SetChained(resolvedArrayExpression, resolvedVal);   
            }
        }
    }
}