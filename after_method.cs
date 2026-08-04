
        _studentRepository.Update(student);
        await _studentRepository.SaveChangesAsync();
    }

    public async Task DeleteStudentAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student with ID {id} not found.");

        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.HasEnrollmentsAsync(id),
            "Cannot delete a student with enrollment history.");

        _studentRepository.Delete(student);
        await _studentRepository.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeStudentId = null)
    {
        return await _studentRepository.EmailExistsAsync(email, excludeStudentId);
    }

    public async Task<StudentDto?> GetStudentByEmailAsync(string email)
    {
        var student = await _studentRepository.GetByEmailAsync(email);
        return student == null ? null : await _studentRepository.GetByIdProjectedAsync(student.StudentId);
    }

    /// <summary>
    /// Checks whether a student is allowed to change from their current status
    /// to the requested status.
    ///
    /// Allowed transitions:
    /// Active → Suspended or Graduated
    /// Suspended → Active
    /// Graduated → cannot change to another status
    ///
    /// Keeping the same status is always allowed.
    /// If the transition is invalid, a BusinessRuleException is thrown.
    /// </summary>

    private static void EnsureValidStatusTransition(StudentStatus current, StudentStatus requested)
    {
        var isValid = current == requested || current switch
        {
            StudentStatus.Active => requested is StudentStatus.Suspended or StudentStatus.Graduated,
            StudentStatus.Suspended => requested == StudentStatus.Active,
            StudentStatus.Graduated => false,
            _ => false
        };

        BusinessRuleHelper.ThrowIfNotExists(
            isValid,
            $"Cannot change student status from {current} to {requested}.");
    }
}
