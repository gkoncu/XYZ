using FluentValidation;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants
{
    public sealed class CreateSystemAnnouncementForAllTenantsCommandValidator
        : AbstractValidator<CreateSystemAnnouncementForAllTenantsCommand>
    {
        public CreateSystemAnnouncementForAllTenantsCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık alanı zorunludur.")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olmalıdır.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("İçerik alanı zorunludur.")
                .MaximumLength(4000).WithMessage("İçerik en fazla 4000 karakter olmalıdır.");

            RuleFor(x => x.PublishDate)
                .NotEmpty()
                .WithMessage("Yayın tarihi alanı zorunludur.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThanOrEqualTo(x => x.PublishDate)
                .When(x => x.ExpiryDate.HasValue)
                .WithMessage("Bitiş tarihi yayın tarihinden önce olamaz.");

            RuleFor(x => x.Type)
                .Equal(AnnouncementType.System)
                .WithMessage("Sistem duyurusu türü System olmalıdır.");
        }
    }
}
