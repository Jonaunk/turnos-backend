using FluentValidation.Results;

namespace Application.Common.Exceptions
{

    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        //Generamos un constructor para obtener las excepciones generales
        public ValidationException() : base("Se han producido uno o más errores de validación")
        {
            Errors = new List<string>();
        }

        //Generamos otro constructor que obtiene la lista de errores de FluentValidation
        public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        {
            foreach (var failure in failures)
            {
                Errors.Add(failure.ErrorMessage);
            }
        }
    }

}