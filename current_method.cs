
        return (await _studentRepository.GetByIdProjectedAsync(student.StudentId))!;
    }

    public async Task UpdateStudentAsync(int id, UpdateStudentDto dto)
    {
        var student = await _studentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student with ID {id} not found.");

        BusinessRuleHelper.ThrowIfExists(
            await _studentRepository.EmailExistsAsync(dto.Email, id),
            $"Student email '{dto.Email}' already exists.");

        EnsureValidStatusTransition(student.Status, dto.Status);

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.Email = dto.Email;
        student.DateOfBirth = dto.DateOfBirth;
        student.PhoneNumber = dto.PhoneNumber;
        student.Status = dto.Status;
