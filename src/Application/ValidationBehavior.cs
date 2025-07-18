namespace Application;

public abstract class ValidationBehavior<TRequest, TResponse>
{
    public abstract Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next);
} 