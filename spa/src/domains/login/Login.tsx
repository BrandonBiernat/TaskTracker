import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../config";
import { useCallback, useState} from "react";
import { Alert, Button, Card, List, ListItem, LoadingOverlay, PasswordInput, Stack, TextInput, Title } from "@mantine/core";
import { useForm } from '@mantine/form';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import MomentumLogo from '/src/assets/momentum logo.png';

type LoginForm = {
    email: string;
    password: string;
}

const loginFormDefaults: LoginForm = {
    email: '',
    password: '',
}

const Login = () => {
    const { login } = useAuth();
    const navigate = useNavigate();
    const [errors, setErrors] = useState<string[]>([]);
    const [isloading, setIsLoading] = useState<boolean>(false); 

    const form = useForm<LoginForm>({
        mode: 'uncontrolled',
        initialValues: loginFormDefaults,
        validate: {
            password: (value) => (value.length < 12 ? 'Password must have at least 12 characters' : null),
            email: (value) => (/^\S+@\S+$/.test(value) ? null : 'Invalid email'),
        },
    });
    
    const handleSubmit = useCallback(async (values: LoginForm) => {
        setIsLoading(true);
        setErrors([]);

        const result = await login(values.email, values.password);

        if (result) {
            setErrors(result);
            setIsLoading(false);
        } else {
            navigate('/');
            setIsLoading(false);
        }
    }, []);

    return (
        <div className="min-h-screen w-full gap-8 flex flex-col items-center justify-center">
            <img
                src={MomentumLogo}
                className="h-20"
            />
            <div className="text-[2rem]">Login to Momentum</div>
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
                <Title order={2}>Welcome Back</Title>
                <form onSubmit={form.onSubmit(handleSubmit)}>
                    <Stack>
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
                        <div className="flex justify-end">
                            <Link 
                                className="text-sm text-blue-600" 
                                to='/forgot-password'
                            >
                                Forgot password?
                            </Link>
                        </div>
                        <Button
                            type='submit'
                            color="purple"
                        >
                            Login
                        </Button>
                    </Stack>
                </form>
                <div className='flex justify-center items-center gap-2'>
                    <span>Don't have an account yet? </span>
                    <Link className="text-blue-600" to='/register'>Register</Link>
                </div>
            </Card>
        </div>
    );
}

export default Login;