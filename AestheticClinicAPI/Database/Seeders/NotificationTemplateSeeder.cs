using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Notifications.Models;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Database.Seeders;

public static class NotificationTemplateSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.NotificationTemplates.AnyAsync())
            return;

        var templates = new List<NotificationTemplate>
        {
            // 1. Appointment Confirmation
            new NotificationTemplate
            {
                Name = "AppointmentConfirmation",
                Subject = "Appointment Confirmed: {{ TreatmentName }} on {{ AppointmentDate }}",
                Content = @"
Dear {{ ClientName }},

Your appointment for **{{ TreatmentName }}** has been confirmed.

📅 **Date:** {{ AppointmentDate }}  
⏰ **Time:** {{ AppointmentTime }}  
⏱️ **Duration:** {{ DurationMinutes }} minutes  
👩‍⚕️ **Staff:** {{ StaffName }}  

📍 **Location:** {{ ClinicAddress }}  

Please arrive 10 minutes early. Bring any relevant medical records.

If you need to reschedule, please contact us at least 24 hours in advance.

Thank you for choosing us!

— {{ ClinicName }} Team
                ".Trim(),
            },
            // 2. Appointment Reminder (1 day before)
            new NotificationTemplate
            {
                Name = "AppointmentReminder",
                Subject = "Reminder: Your {{ TreatmentName }} appointment is tomorrow",
                Content = @"
Dear {{ ClientName }},

This is a friendly reminder that you have an appointment tomorrow.

✨ **Treatment:** {{ TreatmentName }}  
📅 **Date:** {{ AppointmentDate }}  
⏰ **Time:** {{ AppointmentTime }}  
👩‍⚕️ **Staff:** {{ StaffName }}  

Please confirm your attendance by replying to this message or calling us at {{ ClinicPhone }}.

We look forward to seeing you!

— {{ ClinicName }} Team
                ".Trim(),
            },
            // 3. Appointment Rescheduled
            new NotificationTemplate
            {
                Name = "AppointmentRescheduled",
                Subject = "Your appointment has been rescheduled",
                Content = @"
Dear {{ ClientName }},

Your appointment has been rescheduled to the following date/time:

🔄 **Old Schedule:** {{ OldAppointmentDate }} at {{ OldAppointmentTime }}  
✅ **New Schedule:** {{ NewAppointmentDate }} at {{ NewAppointmentTime }}  

**Treatment:** {{ TreatmentName }}  
**Staff:** {{ StaffName }}  

If this new schedule does not work for you, please contact us as soon as possible.

We apologize for any inconvenience.

— {{ ClinicName }} Team
                ".Trim(),
            },
            // 4. Appointment Cancelled
            new NotificationTemplate
            {
                Name = "AppointmentCancelled",
                Subject = "Appointment cancellation notice",
                Content = @"
Dear {{ ClientName }},

Your appointment scheduled on {{ AppointmentDate }} at {{ AppointmentTime }} has been **cancelled**.

**Treatment:** {{ TreatmentName }}  

If you wish to reschedule, please book a new appointment through our portal or call {{ ClinicPhone }}.

We hope to serve you again soon.

— {{ ClinicName }} Team
                ".Trim(),
            },
            // 5. Invoice Sent
            new NotificationTemplate
            {
                Name = "InvoiceSent",
                Subject = "Invoice #{{ InvoiceNumber }} is ready",
                Content = @"
Dear {{ ClientName }},

Your invoice is now available.

🧾 **Invoice #:** {{ InvoiceNumber }}  
📅 **Issue Date:** {{ IssueDate }}  
💰 **Total Amount:** {{ TotalAmount }}  
📆 **Due Date:** {{ DueDate }}  

You can view and pay your invoice by logging into your account.

If you already made a payment, please disregard this message.

— Billing Team
                ".Trim(),
            },
            // 6. Payment Received
            new NotificationTemplate
            {
                Name = "PaymentReceived",
                Subject = "We received your payment",
                Content = @"
Dear {{ ClientName }},

Thank you for your payment!

💰 **Amount:** {{ Amount }}  
🧾 **Invoice #:** {{ InvoiceNumber }}  
📅 **Payment Date:** {{ PaymentDate }}  
💳 **Method:** {{ PaymentMethod }}  

Your updated balance is: {{ BalanceDue }}

If you have any questions, please don't hesitate to contact us.

— Billing Team
                ".Trim(),
            },
            // 7. Overdue Invoice
            new NotificationTemplate
            {
                Name = "InvoiceOverdue",
                Subject = "Payment overdue for invoice #{{ InvoiceNumber }}",
                Content = @"
Dear {{ ClientName }},

Our records indicate that invoice #{{ InvoiceNumber }} is now overdue.

🧾 **Invoice #:** {{ InvoiceNumber }}  
📅 **Due Date:** {{ DueDate }}  
💰 **Outstanding Balance:** {{ BalanceDue }}  

Please make the payment as soon as possible to avoid late fees. You can pay online through your account or visit our clinic.

If you have already paid, please disregard this notice.

— Billing Team
                ".Trim(),
            },
            // 8. Welcome Email (for new client registration)
            new NotificationTemplate
            {
                Name = "WelcomeEmail",
                Subject = "Welcome to {{ ClinicName }}!",
                Content = @"
Dear {{ ClientName }},

Welcome to {{ ClinicName }}! We are delighted to have you with us.

✅ Your account has been successfully created.  
📧 Email: {{ Email }}  
🔗 Login portal: {{ PortalUrl }}  

You can now book appointments, view your treatment history, and manage invoices online.

If you have any questions, feel free to reply to this email or call us at {{ ClinicPhone }}.

Looking forward to providing you with excellent care!

— The {{ ClinicName }} Team
                ".Trim(),
            },
            // 9. Treatment Feedback Request
            new NotificationTemplate
            {
                Name = "FeedbackRequest",
                Subject = "How was your {{ TreatmentName }} experience?",
                Content = @"
Dear {{ ClientName }},

We hope you are satisfied with your recent {{ TreatmentName }} treatment.

We would love to hear your feedback! Please take a moment to rate your experience:

⭐ **Rate us:** {{ FeedbackLink }}  

Your feedback helps us improve our services.

Thank you for choosing {{ ClinicName }}!

— Client Care Team
                ".Trim(),
            },
            // 10. Staff Assignment Notification
            new NotificationTemplate
            {
                Name = "StaffAssigned",
                Subject = "Your appointment is now assigned to {{ StaffName }}",
                Content = @"
Dear {{ ClientName }},

Your upcoming appointment has been assigned to one of our specialists.

👩‍⚕️ **Staff:** {{ StaffName }}  
📅 **Date:** {{ AppointmentDate }}  
⏰ **Time:** {{ AppointmentTime }}  
✨ **Treatment:** {{ TreatmentName }}  

{{ StaffName }} is looking forward to seeing you.

— {{ ClinicName }} Team
                ".Trim(),
            },
            new NotificationTemplate
            {
                Name = "AppointmentRescheduled",
                Subject = "Your appointment has been rescheduled",
                Content = @"
Dear {{ ClientName }},

Your appointment has been rescheduled as follows:

🔄 **Old Schedule:** {{ OldAppointmentDate }} at {{ OldAppointmentTime }}
✅ **New Schedule:** {{ NewAppointmentDate }} at {{ NewAppointmentTime }}

**Treatment:** {{ TreatmentName }}
**Staff:** {{ StaffName }}

If this does not work for you, please contact us at {{ ClinicPhone }}.

— {{ ClinicName }} Team
    ".Trim(),
            },
            new NotificationTemplate
            {
                Name = "EmailChangeRequest",
                Subject = "Your email address has been changed",
                Content = @"
Dear {{ ClientName }},

Your account email address has been changed from {{ OldEmail }} to {{ NewEmail }}.

If you did not make this change, please contact {{ ClinicName }} immediately.

— {{ ClinicName }} Team
    ".Trim(),
            },
            new NotificationTemplate
            {
                Name = "NewReportNotification",
                Subject = "New Report Generated: {{ReportName}}",
                Content = @"
Dear Admin,

A new report has been generated:

**Report Name:** {{ReportName}}
**Generated At:** {{GeneratedAt}}
**Parameters:** {{Parameters}}

**Insights:**
{{Insights}}

Please log into the admin dashboard to view the full report.

— Aesthetic Clinic System
    ".Trim(),
            },
            new NotificationTemplate
            {
                Name = "StaffWelcomeEmail",
                Subject = "Welcome to the team, {{ StaffName }}!",
                Content = @"
Dear {{ StaffName }},

Welcome to {{ ClinicName }} as a {{ Position }}!

Your staff account has been created. Please set your password by clicking the link below:

🔗 **Set your password:** {{ ResetLink }}

This link will expire in 24 hours.

If you did not request this, please ignore this email.

— {{ ClinicName }} Management
    ".Trim(),
            },
            new NotificationTemplate
            {
                Name = "PasswordReset",
                Subject = "Reset Your Password",
                Content =
                    @"
Dear {{ ClientName }},

You requested to reset your password. Click the link below to set a new password:

🔗 {{ ResetLink }}

This link will expire in 24 hours. If you did not request this, please ignore this email.

— Clinic Team",
            },
        };

        await context.NotificationTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
    }
}
