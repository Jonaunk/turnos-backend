using FluentValidation;
using MediatR;

namespace Application.Common.Behaviurs
{
    internal class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        //Creamos en manejador global para canalizar las peticiones
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            //si hay alguna validacion implementada a traves del canal
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);//agarra la peticion entrante
                                                                       //obtenemos los resultados de las validaciones
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                //recolectamos los errores
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();
                //si contiene algun elemento de error
                //if(failures.Count() > 0) 
                if (failures.Any())
                {
                    //generamos una nueva excepcion custom donde le pasamos todos los failures
                    throw new Exceptions.ValidationException(failures);
                }
            }
            //sino que siga
            return await next();
        }
    }
}