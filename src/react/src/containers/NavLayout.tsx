import { MoonIcon, SettingsIcon } from '@chakra-ui/icons';
import {
    Box,
    Button,
    Drawer,
    DrawerBody,
    DrawerCloseButton,
    DrawerContent,
    DrawerOverlay,
    HStack,
    IconButton,
    useColorMode,
    VStack
} from '@chakra-ui/react';
import React, { FC, useState } from 'react';
import { FaSignOutAlt } from 'react-icons/fa';
import { useHistory, useLocation } from 'react-router-dom';
import LogoIcon from '../icons/LogoIcon';
import Auth from '../services/auth';
import './NavLayout.scss';

type NavLayoutProps = {
    onClose: () => void;
    isOpen: boolean;
    variant: string | undefined;
};

const NavLayout: FC<NavLayoutProps> = (props) => {
    const location = useLocation();
    const { colorMode, toggleColorMode } = useColorMode();

    const [activeRoute, setActiveRoute] = useState<string>(location.pathname);

    const history = useHistory();

    const handleNavigate = (page: string) => {
        history.push(page);
        setActiveRoute(page);
    };

    const logout = async () => {
        await Auth.logout();
        history.push('/login');
    };

    const content = (
        <VStack>
            <LogoIcon boxSize={20} />
            <HStack spacing="24px">
                <IconButton
                    aria-label="Switch dark/light mode"
                    title="Switch dark/light mode"
                    icon={<MoonIcon />}
                    onClick={() => toggleColorMode()}
                />
                <IconButton
                    aria-label="Settings"
                    title="Settings"
                    icon={<SettingsIcon />}
                />
                <IconButton
                    aria-label="Sign out"
                    title="Sign out"
                    icon={<FaSignOutAlt />}
                    onClick={() => logout()}
                />
            </HStack>
            <Button
                onClick={() => handleNavigate('/')}
                w="100%"
                isActive={activeRoute === '/'}
            >
                Home
            </Button>
            <Button
                onClick={() => handleNavigate('/servers')}
                w="100%"
                isActive={activeRoute === '/servers'}
            >
                Servers
            </Button>
        </VStack>
    );

    if (props.variant === 'sidebar') {
        return (
            <Box
                position="fixed"
                left={0}
                p={5}
                w="320px"
                top={0}
                h="100%"
                className="sidebar"
            >
                {content}
            </Box>
        );
    }

    return (
        <Drawer isOpen={props.isOpen} placement="left" onClose={props.onClose}>
            <DrawerOverlay>
                <DrawerContent>
                    <DrawerCloseButton />
                    <DrawerBody>{content}</DrawerBody>
                </DrawerContent>
            </DrawerOverlay>
        </Drawer>
    );
};

export default NavLayout;
