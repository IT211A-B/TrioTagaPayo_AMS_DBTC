using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;

        public CourseService(ICourseRepository courseRepository) =>
            _courseRepository = courseRepository;

        public async Task<IEnumerable<CourseResponseDto>> GetAllAsync()
        {
            var courses = await _courseRepository.GetAllWithTeacherAsync();
            return courses.Select(ToDto);
        }

        public async Task<CourseResponseDto?> GetByIdAsync(int id)
        {
            var c = await _courseRepository.GetByIdWithTeacherAsync(id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto)
        {
            var course = new Course
            {
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                Units = dto.Units,
                Section = dto.Section,
                Schedule = dto.Schedule,
                TeacherId = dto.TeacherId
            };
            await _courseRepository.AddAsync(course);
            await _courseRepository.SaveChangesAsync();

            var saved = await _courseRepository.GetByIdWithTeacherAsync(course.Id);
            return ToDto(saved!);
        }

        public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseDto dto)
        {
            var course = await _courseRepository.GetByIdWithTeacherAsync(id);
            if (course == null) return null;

            course.CourseCode = dto.CourseCode;
            course.CourseName = dto.CourseName;
            course.Units = dto.Units;
            course.Section = dto.Section;
            course.Schedule = dto.Schedule;
            course.TeacherId = dto.TeacherId;

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();

            var updated = await _courseRepository.GetByIdWithTeacherAsync(id);
            return ToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null) return false;

            _courseRepository.Remove(course);
            await _courseRepository.SaveChangesAsync();
            return true;
        }

        private static CourseResponseDto ToDto(Course c) => new()
        {
            Id = c.Id,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            Units = c.Units,
            Section = c.Section,
            Schedule = c.Schedule,
            TeacherId = c.TeacherId,
            TeacherName = c.Teacher != null ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "",
            CreatedAt = c.CreatedAt
        };
    }
}