

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


// validation messages that is displayed to the user, if the
//backend doesnt return error message, / backend handles it now
// used before the backend validation was implemented.
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
    passwordConfirm: {
        required: "Confirm password is required",
        match: "Passwords do not match",
    },
};

// function to validate the username
export const validateUsername = (username) => {
    // defines an empty errors array, 
    // that the validation errors get pushed into.
    const errors = [];

    // check if username is provided.
    if (!username || username.trim() === '') {
        // if not, push required error message and return.
        errors.push(validationMessage.username.required);
        return { isValid: false, errors };
    }

    // trims whitespace from the username.
    const trimmed = username.trim();

    // check length of the username and that it is more than min length.
    if (trimmed.length < validationRules.username.minLength) {
        errors.push(validationMessage.username.minLength);
    }
    
    // checks if the username exceeds max length.
    if (trimmed.length > validationRules.username.maxLength) {
        errors.push(validationMessage.username.maxLength);
      }
    
    if (!validationRules.username.pattern.test(trimmed)) {
        errors.push(validationMessage.username.pattern);
    }

    // if there are no errors, return true and the trimmed username.
    return { isValid: errors.length === 0, errors, value: trimmed };
}

// validates the password does the same as username validation.
export const validatePassword = (password) => {
    // defines an empty errors array,
    const errors = [];


    // checks if password is given
    if (!password || password === '') {
        //if not errormessgae is added to the array
      errors.push(validationMessage.password.required);
      return { isValid: false, errors };
    }

    // checks if the password is more than minimum length.
    if (password.length < validationRules.password.minLength) {
      errors.push(validationMessage.password.minLength);
    }

    // checks if the password exceeds maximum length.
    if (password.length > validationRules.password.maxLength) {
      errors.push(validationMessage.password.maxLength);
    }
    
    if(!password.match(validationRules.password.pattern)) {
        errors.push(validationMessage.password.pattern);
    }
    return { isValid: errors.length === 0, errors };
};

// validate login form. used in login page.
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

// validate register form. used in register page.
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

// update account form. used if we have form, but are not used yeet.
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

// validate password confirmation, used in admin and account page. 
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