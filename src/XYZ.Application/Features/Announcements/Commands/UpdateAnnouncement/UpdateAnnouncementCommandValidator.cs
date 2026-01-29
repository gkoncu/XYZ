using FluentValidation;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement
{
    public class UpdateAnnouncementCommandValidator : AbstractValidator<UpdateAnnouncementCommand>
    {
        public UpdateAnnouncementCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Duyuru Id geçersiz.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık alanı zorunludur.")
                .MaximumLength(50).WithMessage("Başlık en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("İçerik alanı zorunludur.")
                .MaximumLength(4000).WithMessage("İçerik en fazla 4000 karakter olmalıdır.");

            RuleFor(x => x.PublishDate)
                .NotEmpty()
                .WithMessage("Yayın tarihi alanı zorunludur.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Tür değeri geçersiz.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue)
                .WithMessage("Sınıf seçimi geçersiz.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThanOrEqualTo(x => x.PublishDate)
                .When(x => x.ExpiryDate.HasValue)
                .WithMessage("Bitiş tarihi yayın tarihinden önce olamaz.");
        }
    }
}
