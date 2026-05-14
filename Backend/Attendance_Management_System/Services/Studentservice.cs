using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository) =>
            _studentRepository = studentRepository;

        public async Task<IEnumerable<StudentResponseDto>> GetAllAsync()
        {
            var students = await _studentRepository.GetAllAsync();
            return students.Select(ToDto);
        }

        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            var s = await _studentRepository.GetByIdAsync(id);
            return s == null ? null : ToDto(s);
        }

        public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
        {
            var student = new Student
            {
                StudentNo = dto.StudentNo,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Email = dto.Email,
                Section = dto.Section,
                MobileNo = dto.MobileNo
            };
            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();
            return ToDto(student);
        }

        public async Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentDto dto)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null) return null;

            student.StudentNo = dto.StudentNo;
            student.FirstName = dto.FirstName;
            student.MiddleName = dto.MiddleName;
            student.LastName = dto.LastName;
            student.Email = dto.Email;
            student.Section = dto.Section;
            student.MobileNo = dto.MobileNo;

            _studentRepository.Update(student);
            await _studentRepository.SaveChangesAsync();
            return ToDto(student);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null) return false;

            _studentRepository.Remove(student);
            await _studentRepository.SaveChangesAsync();
            return true;
        }

        private static StudentResponseDto ToDto(Student s) => new()
        {
            Id = s.Id,
            StudentNo = s.StudentNo,
            FirstName = s.FirstName,
            MiddleName = s.MiddleName,
            LastName = s.LastName,
            Email = s.Email,
            Section = s.Section,
            MobileNo = s.MobileNo,
            CreatedAt = s.CreatedAt
        };
    }
}