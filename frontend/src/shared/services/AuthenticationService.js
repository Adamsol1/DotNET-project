/** @typedef {import('..types/Auth').LoginUserDto} LoginUserDto */
/** @typedef {import('..types/Auth').RegistrerUserDto} RegistrerUserDto */
import {auth} from "./api";

export async function login(Username, Password) {
    const payload = {Username, Password};
    return auth.login(payload);
}

export async function register(username, password) {
    const payload = {username, password};
    return auth.register(payload);
}


