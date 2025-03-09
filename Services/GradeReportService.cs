using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Data;
using SchoolManagementSystem.Models;
using SelectPdf;
using System.Text;

namespace SchoolManagementSystem.Services
{
    public class GradeReportService
    {
        public byte[] GenerateGradeReportPdf(int studentId, ApplicationDbContext context)
        {
            var grades = context.Grades
                .Include(g => g.Student).ThenInclude(s => s.User)
                .Include(g => g.Teacher).ThenInclude(t => t.User)
                .Where(g => g.StudentId == studentId)
                .ToList();

            if (!grades.Any())
                throw new Exception("No grades found.");

            var htmlContent = new StringBuilder();
            htmlContent.Append(@"
            <html>
            <head>
                <style>
                    body { 
                        font-family: Arial, sans-serif; 
                        margin: 0; 
                        padding: 20px; 
                        position: relative; 
                    }
                    h1 { text-align: center; margin: 10px 0; }
                    h3 { color: #800000; margin-bottom: 15px; }
                    .table-responsive { margin-bottom: 30px; position: relative; z-index: 1; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { padding: 8px; text-align: left; border: 1px solid #ddd; }
                    .table-dark th { background-color: #800000; color: #F5F5F5; }
                    .table-striped tr:nth-child(even) { background-color: #f2f2f2; }
                    .text-success { color: #28a745; font-weight: bold; }
                    tfoot td { font-weight: bold; }
                    .school-header { text-align: center; margin-bottom: 20px; }
                    .school-header p { margin: 2px 0; }
                    .report-info { text-align: right; font-size: 12px; margin-bottom: 20px; }
                </style>
            </head>
            <body>
                <div class='school-header'>
                    <h1>Sanchez Mira School of Arts and Trades</h1>
                    <p>Maharlika North Road, Brgy. Santor, Sanchez Mira 3518 Cagayan</p>
                    <p>Contact: (078) 304-0073 | Email: 300486@deped.gov.ph</p>
                </div>
                <div class='report-info'>
                    <p>Generated on: " + DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt") + @"</p>
                </div>
                <h1>Grade Report</h1>
                <p><strong>Student:</strong> " + grades[0].Student.User.FirstName + " " + grades[0].Student.User.LastName + @"</p>
                <p><strong>LRN:</strong> " + grades[0].Student.LRN + @"</p>");

            // First Semester Section
            htmlContent.Append(@"
            <div class='table-responsive'>
                <h3>First Semester</h3>
                <table>
                    <thead class='table-dark'>
                        <tr>
                            <th>Subject</th>
                            <th>Subject Type</th>
                            <th>Quarter 1</th>
                            <th>Quarter 2</th>
                            <th>Final Grade</th>
                            <!--<th>Teacher</th>-->
                        </tr>
                    </thead>
                    <tbody>");

            var firstSemesterGrades = grades.Where(g => g.Semester == SemesterType.First).ToList();
            if (firstSemesterGrades.Any())
            {
                foreach (var grade in firstSemesterGrades)
                {
                    var finalGrade = (grade.Quarter1.HasValue && grade.Quarter2.HasValue) ? (grade.Quarter1 + grade.Quarter2) / 2.0 : null;
                    htmlContent.Append($@"
                        <tr>
                            <td>{grade.Subject}</td>
                            <td>{grade.SubjectType}</td>
                            <td>{grade.Quarter1?.ToString() ?? "N/A"}</td>
                            <td>{grade.Quarter2?.ToString() ?? "N/A"}</td>
                            <td class='text-success'>{(finalGrade.HasValue ? finalGrade.Value.ToString("F2") : "N/A")}</td>
                            <!--<td>{(grade.Teacher?.User != null ? $"{grade.Teacher.User.FirstName} {grade.Teacher.User.LastName}" : "Unknown")}</td>-->
                        </tr>");
                }
            }
            else
            {
                htmlContent.Append("<tr><td colspan='6'>No grades recorded for First Semester.</td></tr>");
            }

            var firstSemAverage = firstSemesterGrades
                .Select(g => (g.Quarter1.HasValue && g.Quarter2.HasValue) ? (g.Quarter1 + g.Quarter2) / 2.0 : (double?)null)
                .Where(g => g.HasValue)
                .Average();

            if(firstSemesterGrades.Any())
            {
                htmlContent.Append($@"
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='4'>General Average for the semester</td>
                            <td class='text-success'>{(firstSemAverage.HasValue ? firstSemAverage.Value.ToString("F2") : "N/A")}</td>
                            <!--<td></td>-->
                        </tr>
                    </tfoot>
                </table>
            </div>");

            }
            

            // Second Semester Section
            htmlContent.Append(@"
            <div class='table-responsive'>
                <h3>Second Semester</h3>
                <table>
                    <thead class='table-dark'>
                        <tr>
                            <th>Subject</th>
                            <th>Subject Type</th>
                            <th>Quarter 3</th>
                            <th>Quarter 4</th>
                            <th>Final Grade</th>
                            <!--<th>Teacher</th>-->
                        </tr>
                    </thead>
                    <tbody>");

            var secondSemesterGrades = grades.Where(g => g.Semester == SemesterType.Second).ToList();
            if (secondSemesterGrades.Any())
            {
                foreach (var grade in secondSemesterGrades)
                {
                    var finalGrade = (grade.Quarter3.HasValue && grade.Quarter4.HasValue) ? (grade.Quarter3 + grade.Quarter4) / 2.0 : null;
                    htmlContent.Append($@"
                        <tr>
                            <td>{grade.Subject}</td>
                            <td>{grade.SubjectType}</td>
                            <td>{grade.Quarter3?.ToString() ?? "N/A"}</td>
                            <td>{grade.Quarter4?.ToString() ?? "N/A"}</td>
                            <td class='text-success'>{(finalGrade.HasValue ? finalGrade.Value.ToString("F2") : "N/A")}</td>
                           <!-- <td>{(grade.Teacher?.User != null ? $"{grade.Teacher.User.FirstName} {grade.Teacher.User.LastName}" : "Unknown")}</td> -->
                        </tr>");
                }
            }
            else
            {
                htmlContent.Append("<tr><td colspan='6'>No grades recorded for Second Semester.</td></tr>");
            }

            var secondSemAverage = secondSemesterGrades
                .Select(g => (g.Quarter3.HasValue && g.Quarter4.HasValue) ? (g.Quarter3 + g.Quarter4) / 2.0 : (double?)null)
                .Where(g => g.HasValue)
                .Average();

            if (secondSemesterGrades.Any())
            {
                htmlContent.Append($@"
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='4'>General Average for the semester</td>
                            <td class='text-success'>{(secondSemAverage.HasValue ? secondSemAverage.Value.ToString("F2") : "N/A")}</td>
                            <!--<td></td>-->
                        </tr>
                    </tfoot>
                </table>
            </div>
            </body>
            </html>");
            }
                

            HtmlToPdf converter = new HtmlToPdf();

            // Set margins (in points; 1 inch = 72 points)
            converter.Options.MarginTop = 36;    // 0.5 inch
            converter.Options.MarginBottom = 36; // 0.5 inch
            converter.Options.MarginLeft = 36;   // 0.5 inch
            converter.Options.MarginRight = 36;  // 0.5 inch

            PdfDocument doc = converter.ConvertHtmlString(htmlContent.ToString());
            var stream = new MemoryStream();
            doc.Save(stream);
            doc.Close();
            return stream.ToArray();
        }
    }
}