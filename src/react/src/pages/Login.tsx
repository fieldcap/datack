import {
    Alert,
    AlertIcon,
    Box,
    Button,
    chakra,
    Checkbox,
    Flex,
    FormControl,
    FormHelperText,
    Heading,
    Input,
    InputGroup,
    InputLeftElement,
    Link,
    Spinner,
    Stack,
    useColorMode,
    useColorModeValue
} from '@chakra-ui/react';
import React, { FC, useEffect, useState } from 'react';
import { FaLock, FaUserAlt } from 'react-icons/fa';
import { Redirect, RouteComponentProps } from 'react-router-dom';
import LogoIcon from '../icons/LogoIcon';
import Auth from '../services/auth';
import './Login.scss';

const CFaUserAlt = chakra(FaUserAlt);
const CFaLock = chakra(FaLock);

const Login: FC<RouteComponentProps> = () => {
    const { colorMode, toggleColorMode } = useColorMode();
    const bg = useColorModeValue('whiteAlpha.900', 'gray.700');

    const [email, setEmail] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const [rememberMe, setRememberMe] = useState<boolean>(true);
    const [isSetup, setIsSetup] = useState<boolean>(false);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [isLoggingIn, setIsLoggingIn] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [redirect, setRedirect] = useState<boolean>(false);

    useEffect(() => {
        (async () => {
            const result = await Auth.isSetup();

            setIsLoading(false);
            setIsSetup(result);
        })();
    }, []);

    const validateForm = () => {
        return email.length > 0 && password.length > 0;
    };

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        setError(null);
        setIsLoggingIn(true);

        try {
            await Auth.login(email, password, rememberMe);

            setIsLoggingIn(false);
            setRedirect(true);
        } catch (err) {
            setError(err);
            setIsLoggingIn(false);
        }
    };

    if (redirect) {
        return <Redirect to="/" />;
    }

    const loadingForm = (
        <>
            <Flex
                flexDirection="column"
                width="100%"
                justifyContent="center"
                alignItems="center"
            >
                <LogoIcon boxSize={20} />
                <Heading>Datack SQL</Heading>
                <Spinner />
            </Flex>
        </>
    );

    const loginForm = (
        <>
            <Flex
                flexDirection="column"
                width="100%"
                justifyContent="center"
                alignItems="center"
            >
                <LogoIcon boxSize={20} />
                <Heading>Datack SQL</Heading>
            </Flex>
            {!isSetup ? (
                <Alert status="info">
                    Welcome to Datack! <br />
                    To setup the first account please enter a username and
                    password to continue.
                </Alert>
            ) : null}
            <FormControl>
                <InputGroup>
                    <InputLeftElement
                        pointerEvents="none"
                        children={<CFaUserAlt />}
                    />
                    <Input
                        type="text"
                        placeholder="Username"
                        autoFocus={true}
                        onChange={(e) => setEmail(e.target.value)}
                    />
                </InputGroup>
            </FormControl>
            <FormControl>
                <InputGroup>
                    <InputLeftElement
                        pointerEvents="none"
                        children={<CFaLock />}
                    />
                    <Input
                        type="password"
                        placeholder="Password"
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </InputGroup>
                <FormHelperText textAlign="right">
                    <Link>forgot password?</Link>
                </FormHelperText>
            </FormControl>
            <FormControl>
                <InputGroup>
                    <Checkbox
                        defaultIsChecked
                        onChange={(e) => setRememberMe(e.target.checked)}
                    >
                        Remember me?
                    </Checkbox>
                </InputGroup>
            </FormControl>
            {error != null ? (
                <Alert status="error">
                    <AlertIcon />
                    {error}
                </Alert>
            ) : null}
            <Button
                borderRadius={0}
                type="submit"
                width="full"
                disabled={!validateForm() || isLoggingIn}
                isLoading={isLoggingIn}
            >
                Login
            </Button>
        </>
    );

    return (
        <Flex
            flexDirection="column"
            width="100wh"
            height="100vh"
            justifyContent="center"
            alignItems="center"
            className="login-container"
        >
            <Stack
                flexDir="column"
                mb="2"
                justifyContent="center"
                alignItems="center"
            >
                <Box minW={{ base: '90%', md: '468px' }}>
                    <form onSubmit={handleSubmit}>
                        <Stack
                            spacing={4}
                            p="1rem"
                            boxShadow="md"
                            backgroundColor={bg}
                        >
                            {isLoading ? loadingForm : loginForm}
                        </Stack>
                    </form>
                </Box>
            </Stack>
        </Flex>
    );
};

export default Login;
