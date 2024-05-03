using PasswordStorageService.Data;

namespace PasswordStorageService.Services.PasswordStorage;

public partial class PasswordStorageController(PasswordStorageContext database)
    : PasswordManager.PasswordStorageService.PasswordStorageServiceBase
{
}