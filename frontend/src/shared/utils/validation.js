

/**
 * Validation module, for the frontend on user input,
 * and error handling / live feedback to the user.
 */

export const validationRules = {
    // decide how long and regex pattern on password and username.
    username: {
        minLength: 3,
        maxLength: 20,
        // can contain only letters, numbers, and underscores.
        pattern: /^[a-zA-Z0-9_]+$/,
        required: true,
    },

    password: {
        minLength: 8,
        // setting max to 50
        maxLength: 50,
        // not allow ; -- " ' / \ etc.
        pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=.,:?])[A-Za-z0-9!@#$%^&*()_\-+=.,:?]{8,50}$/,
        required: true,
        requiredUppercase: true,
        requiredDigit: true,
        requiredSpecialChar: true,
    },
}

export const validationMessage = {
    username: {
        required: "Username is required",
        minLength: "Username must be at least 3 characters long",
        maxLength: "Username must be less than 20 characters long",
        pattern: "Username can only contain letters, numbers, and underscores",

    },
    password: {
        required: "Password is required",
        minLength: "Password must be at least 8 characters long",
        maxLength: "Password must be less than 50 characters long",
        pattern: "Password must contain atleast one: uppercase letter, digit, and a special character (!#% etc.)",
    },
};

// function to validate the username.
export const validateUsername = (username) => {
    const errors = [];

    if (!username || username.trim() === '') {
        errors.push(validationMessage.username.required);
        return { isValid: false, errors };
    }

    const trimmed = username.trim();

    if (trimmed.length < validationRules.username.minLength) {
        errors.push(validationMessage.username.minLength);
    }
    
    if (trimmed.length > validationRules.username.maxLength) {
        errors.push(validationMessage.username.maxLength);
      }
    
    if (!validationRules.username.pattern.test(trimmed)) {
        errors.push(validationMessage.username.pattern);
    }

    // if there are no errors, return true and the trimmed username.
    return { isValid: errors.length === 0, errors, value: trimmed };
}

export const validatePassword = (password) => {
    const errors = [];
  
    if (!password || password === '') {
      errors.push(validationMessage.password.required);
      return { isValid: false, errors };
    }
  
    if (password.length < validationRules.password.minLength) {
      errors.push(validationMessage.password.minLength);
    }
  
    if (password.length > validationRules.password.maxLength) {
      errors.push(validationMessage.password.maxLength);
    }
    
    if(!password.match(validationRules.password.pattern)) {
        errors.push(validationMessage.password.pattern);
    }
    return { isValid: errors.length === 0, errors };
};

// validate login form.
export const validateLoginForm = (username, password) => {
    const usernameValidation = validateUsername(username);
    const passwordValidation = validatePassword(password);

    const errors = {
        username: usernameValidation.errors,
        password: passwordValidation.errors,
    };

    const isValid = usernameValidation.isValid && passwordValidation.isValid;

    return { isValid, errors, 
        values: { username: usernameValidation.value, password: passwordValidation.value },
    };
};

// validate register form.
export const validateRegisterForm = (username, password) => {
    const usernameValidation = validateUsername(username);
    const passwordValidation = validatePassword(password);

    const errors = {
        username: usernameValidation.errors,
        password: passwordValidation.errors,
    };

    const isValid = usernameValidation.isValid && passwordValidation.isValid;

    return { isValid, errors, 
        values: { username: usernameValidation.value, password: passwordValidation.value },
    };
};

// update account form.
export const validateUpdateAccountForm = (username, password, confirmPassword) => {
    const usernameValidation = validateUsername(username);
    const passwordValidation = validatePassword(password);
    const passwordConfirmationValidation = validatePasswordConfirmation(password, confirmPassword);

    const errors = {
        username: usernameValidation.errors,
        password: passwordValidation.errors,
        passwordConfirmation: passwordConfirmationValidation.errors,
    };

    const isValid = usernameValidation.isValid && passwordValidation.isValid && passwordConfirmationValidation.isValid;

    return { isValid, errors, 
        values: { username: usernameValidation.value, password: passwordValidation.value, passwordConfirmation: passwordConfirmationValidation.value },
    };
};

export const validatePasswordConfirmation = (password, confirmPassword) => {
    const errors = [];
  
    if (!confirmPassword || confirmPassword === '') {
      errors.push(validationMessage.passwordConfirm.required);
      return { isValid: false, errors };
    }
  
    if (password !== confirmPassword) {
      errors.push(validationMessage.passwordConfirm.match);
    }
  
    return { isValid: errors.length === 0, errors };
};