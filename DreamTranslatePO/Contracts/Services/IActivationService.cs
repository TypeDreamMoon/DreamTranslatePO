namespace DreamTranslatePO.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
