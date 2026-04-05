import { Alert, Button, Card, List, ListItem, LoadingOverlay, PasswordInput, Stack, TextInput, Title } from '@mantine/core';
import MomentumLogo from '/src/assets/momentum logo.png';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../config';
import { useCallback, useState } from 'react';
import { useForm } from '@mantine/form';

type RegisterForm = {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
}

const registerFormDefaults: RegisterForm = {
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
}

const Register = () => {
    const { register, login } = useAuth();
    const navigate = useNavigate();
    const [errors, setErrors] = useState<string[]>([]);
    const [isloading, setIsLoading] = useState<boolean>(false);

    const form = useForm<RegisterForm>({
        mode: 'uncontrolled',
        initialValues: registerFormDefaults,
        validate: {
            password: (value) => (value.length < 12 ? 'Password must have at least 12 characters' : null),
            confirmPassword: (value, values) => (value !== values.password ? 'Passwords do not match' : null),
            email: (value) => (/^\S+@\S+$/.test(value) ? null : 'Invalid email'),
            firstName: (value) => (value.length <= 0 ? 'Must enter first name' : null),
            lastName: (value) => (value.length <= 0 ? 'Must enter last name' : null),
        },
    });

    const handleSubmit = useCallback(async (values: RegisterForm) => {
        setIsLoading(true);
        setErrors([]);

        const result = await register(values.email, values.password, values.firstName, values.lastName);

        if (result) {
            setErrors(result);
            setIsLoading(false);
            return;
        }

        const loginResult = await login(values.email, values.password);

        if (loginResult) {
            setErrors(loginResult);
            setIsLoading(false);
        } else {
            navigate('/');
        }
    }, []);
    
    return (
        <div className="min-h-screen w-full gap-8 flex flex-col items-center justify-center bg-[#f8f7fc]">
            <img 
                src={MomentumLogo}
                className="h-20"
            />
            <div className="text-[2rem]">Join Momentum</div>
            <Card shadow="md" className='flex flex-col gap-5 w-full max-w-lg p-10'>
                <LoadingOverlay 
                    visible={isloading} 
                    loaderProps={{ type: 'bars' }} 
                />
                {errors.length > 0 && (
                    <Alert 
                        color="red" 
                        icon={<FontAwesomeIcon icon={faTriangleExclamation} />}
                    >
                        <List>
                            {errors.map((err, index) => (
                                <ListItem key={index}>{err}</ListItem>
                            ))} 
                        </List>
                    </Alert>
                )}
                <Title order={2}>Get Started</Title>
                <form onSubmit={form.onSubmit(handleSubmit)}>
                    <Stack>
                        <div className='flex w-full gap-2'>
                            <TextInput
                                className='flex-1'
                                label='First Name'
                                placeholder='Your First Name'
                                key={form.key('firstName')}
                                {...form.getInputProps('firstName')}
                            />
                            <TextInput
                                className='flex-1'
                                label='Last Name'
                                placeholder='Your Last Name'
                                key={form.key('lastName')}
                                {...form.getInputProps('lastName')}
                            />
                        </div>
                        <TextInput
                            label='Email'
                            placeholder="Email"
                            key={form.key('email')}
                            {...form.getInputProps('email')}
                        />
                        <PasswordInput
                            label='Password'
                            placeholder="Password"
                            key={form.key('password')}
                            {...form.getInputProps('password')}
                        />
                        <PasswordInput
                            label='Confirm Password'
                            placeholder="Confirm Password"
                            key={form.key('confirmPassword')}
                            {...form.getInputProps('confirmPassword')}
                        />
                        <Button
                            type='submit'
                            color="purple"
                        >
                            Login
                        </Button>
                    </Stack>
                </form>
                <div className='flex justify-center items-center gap-2'>
                    <span>Already have an account? </span>
                    <Link 
                        className="text-blue-600" 
                        to='/login'
                    >
                        Login
                    </Link>
                </div>
            </Card>
        </div>
    );
}

export default Register;